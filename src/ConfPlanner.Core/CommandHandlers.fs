module CommandHandlers

open Chessie.ErrorHandling
open Domain
open Commands
open Events
open States
open Errors


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

let (|OrganizerExceededMaxNumbersOfVotes|_|) votingResults (MaxVotesPerOrganizer maxVotes) (voting : Voting) =
  let isVoteOfVoter voterId  = function
    | Voting.Vote (_,id) -> id = voterId
    | _ -> false

  let number =
    votingResults
    |> List.filter (isVoteOfVoter <| extractVoterId voting)
    |> List.length

  match number >= maxVotes with
  | true -> Some voting
  | false -> None

let (|OrganizerExceededMaxNumbersOfVetos|_|) votingResults (MaxVetosPerOrganizer maxVetos) (voting : Voting) =
  let isVetoOfVoter voterId  = function
    | Voting.Veto (_,id) -> id = voterId
    | _ -> false

  let number =
    votingResults
    |> List.filter (isVetoOfVoter <| extractVoterId voting)
    |> List.length

  match number >= maxVetos with
  | true -> Some voting
  | false -> None

let handleProposeAbstract state proposed =
  match state.CallForPapers with
  | Open -> [AbstractWasProposed proposed] |> ok
  | NotOpened -> CallForPapersNotOpened |> fail
  | Closed -> CallForPapersClosed |> fail

let score m (abstr : AbstractId) =
    match m |> Map.tryFind abstr with
    | Some value -> m |> Map.add abstr (value + 1)
    | None -> m |> Map.add abstr 1

let scoreAbstracts state =
  let talks,handsOns =
    state.Abstracts
    |> List.partition (fun abstr -> abstr.Type = Talk)

  let votes,vetos =
    state.VotingResults
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
      let events =
        [VotingPeriodWasFinished]
        |> (@) (scoreAbstracts state)

      // printfn "actual %A" events
      events
      |> ok

  | Closed,Finished ->
      VotingPeriodAlreadyFinished |> fail

  | _,_ ->
      CallForPapersNotClosed |> fail

let handleVote state voting =
  match state.VotingPeriod with
  | Finished ->
      VotingPeriodAlreadyFinished |> fail
  | InProgress ->
      match voting with
      | VoterIsNotAnOrganizer state.Organizers _ ->
          VoterIsNotAnOrganizer |> fail

      | AlreadyVotedForAbstract state.VotingResults _ ->
          VotingAlreadyIssued |> fail

      | OrganizerExceededMaxNumbersOfVotes state.VotingResults state.MaxVotesPerOrganizer _ ->
          MaxNumberOfVotesExceeded |> fail

      | OrganizerExceededMaxNumbersOfVetos state.VotingResults state.MaxVetosPerOrganizer _ ->
          MaxNumberOfVetosExceeded |> fail

      | _ ->
          [VotingWasIssued voting] |> ok

let handleRevokeVoting state voting =
  match state.VotingPeriod with
  | Finished ->
      VotingPeriodAlreadyFinished |> fail
  | InProgress ->
      match voting with
      | DidNotVoteForAbstract state.VotingResults _ ->
          OrganizerDidNotVoteForAbstract |> fail

      | _ ->
          [VotingWasRevoked voting] |> ok


let execute (state: State) (command: Command) : Result<Event list, Error> =
  match command with
  | ProposeAbstract proposed -> handleProposeAbstract state proposed
  | FinishVotingPeriod -> handleFinishVotingPeriod state
  | Vote voting -> handleVote state voting
  | RevokeVoting voting -> handleRevokeVoting state voting


let evolve (state : State) (command : Command) =
  match execute state command with
  | Ok (events,_) ->
      let newState = List.fold States.apply state events
      (newState, events) |> ok

  | Bad error -> Bad error