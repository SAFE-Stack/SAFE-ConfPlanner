module CommandHandlers

open Chessie.ErrorHandling
open Domain
open Commands
open Events
open States
open Errors


let handleProposeAbstract state proposed =
  match state.CallForPapers with
  | Open -> [AbstractWasProposed proposed] |> ok
  | NotOpened -> CallForPapersNotOpened |> fail
  | Closed -> CallForPapersClosed |> fail

let handleFinishVotingPeriod state =
  match state.VotingPeriod with
  | InProgess -> [VotingPeriodWasFinished] |> ok
  | _ -> VotingPeriodAlreadyFinished |> fail

let execute (state: State) (command: Command) : Result<Event list, Error> =
  match command with
  | ProposeAbstract proposed -> handleProposeAbstract state proposed
  | FinishVotingPeriod -> handleFinishVotingPeriod state


let evolve (state : State) (command : Command) =
  match execute state command with
  | Ok (events,_) ->
      let newState = List.fold States.apply state events
      (newState, events) |> ok

  | Bad error -> Bad error