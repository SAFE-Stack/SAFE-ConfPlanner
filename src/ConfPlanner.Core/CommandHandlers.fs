module CommandHandlers

open Chessie.ErrorHandling
open Domain
open Commands
open Events
open States
open Errors


let handleFinishVotingPeriod state =
  match state.VotingPeriod with
  | InProgess -> [VotingPeriodWasFinished] |> ok
  | _ -> VotingPeriodAlreadyFinished |> fail

let execute (state: State) (command: Command) : Result<Event list, Error> =
  match command with
  | FinishVotingPeriod -> handleFinishVotingPeriod state
  | _ -> [] |> ok

let evolve (state : State) (command : Command) =
  match execute state command with
  | Ok (events,_) ->
      let newState = List.fold States.apply state events
      (newState, events) |> ok

  | Bad error -> Bad error