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

let (|VoterIsNotAnOrganizer|_|) (organizers: Organizer list) (voting : Voting) =
  let isNotOrganizer voting =
    organizers
    |> List.map (fun x -> x.Id)
    |> List.contains (extractVoterId voting)
    |> not

  match isNotOrganizer voting with
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

let handleFinishVotingPeriod state =
  match state.VotingPeriod with
  | InProgess -> [VotingPeriodWasFinished] |> ok
  | _ -> VotingPeriodAlreadyFinished |> fail

let handleVoting state voting =
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


let execute (state: State) (command: Command) : Result<Event list, Error> =
  match command with
  | ProposeAbstract proposed -> handleProposeAbstract state proposed
  | FinishVotingPeriod -> handleFinishVotingPeriod state
  | Vote voting -> handleVoting state voting


let evolve (state : State) (command : Command) =
  match execute state command with
  | Ok (events,_) ->
      let newState = List.fold States.apply state events
      (newState, events) |> ok

  | Bad error -> Bad error