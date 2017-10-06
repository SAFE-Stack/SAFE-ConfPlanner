module Behaviour

open System
open Model
open Commands
open Events
open Projections


let (|AlreadyVotedForAbstract|_|) votingResults voting =
  match List.contains voting votingResults with
  | true -> Some voting
  | false -> None

let (|DidNotVoteForAbstract|_|) votingResults voting =
  match not <| List.contains voting votingResults with
  | true -> Some voting
  | false -> None

let (|VoterIsNotAnOrganizer|_|) (organizers: Organizer list) (voting : Voting) =
  let isNotOrganizer voting =
    organizers
    |> List.map (fun x -> x.Id)
    |> List.contains (extractVoterId voting)
    |> not

  match isNotOrganizer voting with
  | true -> Some voting
  | false -> None

let numberOfVotesExceeded votingResults getVote (voting : Voting) max =
  let number =
    votingResults
    |> List.filter (getVote <| extractVoterId voting)
    |> List.length

  match number >= max with
  | true -> Some voting
  | false -> None

let handleProposeAbstract givenHistory proposed =
  match (conferenceState givenHistory).CallForPapers with
  | Open -> [AbstractWasProposed proposed]
  | NotOpened -> [ProposingDenied "Call For Papers Not Opened"]
  | Closed -> [ProposingDenied "Call For Papers Closed"]

let score m (abstr : AbstractId) =
    match m |> Map.tryFind abstr with
    | Some value -> m |> Map.add abstr (value + 1)
    | None -> m |> Map.add abstr 1

let scoreAbstracts state =
  let talks,handsOns =
    state.Abstracts
    |> List.partition (fun abstr -> abstr.Type = Talk)

  let votes,vetos =
    state.Votings
    |> List.partition (function | Voting.Vote _ -> true | _ -> false)

  let abstractsWithVetos =
    vetos
    |> List.map extractAbstractId

  let withoutVetos =
    votes
    |> List.map extractAbstractId
    |> List.filter (fun abstractId -> abstractsWithVetos |> List.contains abstractId |> not)

  let sumPoints abstractId =
    votes
      |> List.map extractPoints
      |> List.filter (fun (id,_) -> id = abstractId)
      |> List.map (fun (id,points) -> points)
      |> List.sumBy (fun points -> match points with | Zero -> 0| One -> 1 | Two -> 2)

  let accepted =
    withoutVetos
    |> Seq.sortByDescending sumPoints
    |> Seq.distinct
    |> Seq.truncate state.AvailableSlotsForTalks
    |> Seq.toList

  let rejected =
    talks
    |> List.map (fun abstr -> abstr.Id)
    |> List.filter (fun id -> accepted |> List.contains id |> not)

  accepted
  |> List.map AbstractWasAccepted
  |> (@) (rejected |> List.map AbstractWasRejected)

let finishVotingPeriod conference =
  match conference.CallForPapers,conference.VotingPeriod with
  | Closed,InProgess ->
      let unfinishedVotings =
        conference.Abstracts
        |> Seq.map (fun abstr ->
            conference.Votings
            |> List.map extractAbstractId
            |> List.filter (fun id -> id = abstr.Id)
            |> List.length)
        |> Seq.filter (fun votes ->
            votes <> conference.Organizers.Length)
        |> Seq.length
      let events =
        match unfinishedVotings with
        | 0 ->
            [VotingPeriodWasFinished]
            |> (@) (scoreAbstracts conference)
        | _ -> [FinishingDenied "Not all abstracts have been voted for by all organisers"]
      events

  | Closed,Finished -> [FinishingDenied "Voting Period Already Finished"]

  | _,_ -> [FinishingDenied "Call For Papers Not Closed"]

let handleFinishVotingPeriod givenHistory =
  givenHistory
  |> conferenceState
  |> finishVotingPeriod

let reopenVotingPeriod conference =
  match conference.CallForPapers,conference.VotingPeriod with
  | Closed,Finished ->
      [VotingPeriodWasReopened]

  | _,_ ->
    [FinishingDenied "Call For Papers Not Closed"]

let handleReopenVotingPeriod givenHistory =
  givenHistory
  |> conferenceState
  |> reopenVotingPeriod

let handleVote givenHistory voting =
  let state = (conferenceState givenHistory)
  match state.VotingPeriod with
  | Finished -> [VotingDenied "Voting Period Already Finished"]
  | InProgress ->
    match voting with
    | VoterIsNotAnOrganizer state.Organizers _ ->
        [VotingDenied "Voter Is Not An Organizer"]

    | _ -> [VotingWasIssued voting]

let execute (given_history: Event list) (command: Command) : Event list =
  match command with
  | ProposeAbstract proposed -> handleProposeAbstract given_history proposed
  | FinishVotingPeriod -> handleFinishVotingPeriod given_history
  | ReopenVotingPeriod -> handleReopenVotingPeriod given_history
  | Vote voting -> handleVote given_history voting
  | RevokeVoting(_) -> failwith "Not Implemented"
  | AcceptAbstract(_) -> failwith "Not Implemented"
  | RejectAbstract(_) -> failwith "Not Implemented"
