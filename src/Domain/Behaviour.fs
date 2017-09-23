module Behaviour

open Model
open Commands
open Events
open States


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

let handleProposeAbstract state proposed =
  match state.CallForPapers with
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
    |> List.fold score Map.empty
    |> Map.toList
    |> List.sortBy (fun (_, votes) -> votes)
    |> List.map fst
    |> List.filter (fun abstractId -> abstractsWithVetos |> List.contains abstractId |> not)
    |> List.rev

  let accepted =
    withoutVetos
    |> Seq.truncate state.AvailableSlotsForTalks
    |> Seq.toList

  let rejected =
    talks
    |> List.map (fun abstr -> abstr.Id)
    |> List.filter (fun id -> accepted |> List.contains id |> not)

  accepted
  |> List.map AbstractWasAccepted
  |> (@) (rejected |> List.map AbstractWasRejected)

let handleFinishVotingPeriod state =
  match state.CallForPapers,state.VotingPeriod with
  | Closed,InProgess ->
      let unfinishedVotings =
        state.Abstracts
        |> Seq.map (fun abstr ->
            state.Votings
            |> List.map extractAbstractId
            |> List.filter (fun id -> id = abstr.Id)
            |> List.length)
        |> Seq.filter (fun votes ->
            votes <> state.Organizers.Length)
        |> Seq.length
      let events =
        match unfinishedVotings with
        | 0 ->
            [VotingPeriodWasFinished]
            |> (@) (scoreAbstracts state)
        | _ ->
            [FinishingDenied "Not all abstracts have been voted for by all organisers"]
      events
  | Closed,Finished ->
      [FinishingDenied "Voting Period Already Finished"]
  | _,_ ->
      [FinishingDenied "Call For Papers Not Closed"]

let handleVote state voting =
  match state.VotingPeriod with
  | Finished ->
      [VotingDenied "Voting Period Already Finished"]
  | InProgress ->
      match voting with
      | VoterIsNotAnOrganizer state.Organizers _ ->
          [VotingDenied "Voter Is Not An Organizer"]
      | _ ->
          [VotingWasIssued voting]

let execute (state: State) (command: Command) : Event list =



  match command with
  | ProposeAbstract proposed -> handleProposeAbstract state proposed
  | FinishVotingPeriod -> handleFinishVotingPeriod state
  | Vote voting -> handleVote state voting
  | RevokeVoting(_) -> failwith "Not Implemented"
  | AcceptAbstract(_) -> failwith "Not Implemented"
  | RejectAbstract(_) -> failwith "Not Implemented"
