module CommandHandler

open Chessie.ErrorHandling
open Domain
open Commands
open Events
open States


let execute state command =
  match command with
  | _ -> [] |> ok

let evolve (state : State) (command : Command) =
  match execute state command with
  | Ok (events,_) ->
      let newState = List.fold States.apply state events
      (newState, events) |> ok

  | Bad error -> Bad error