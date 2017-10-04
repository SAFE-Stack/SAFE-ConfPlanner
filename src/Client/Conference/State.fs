module Conference.State

open Elmish
open Global

open Server.ServerTypes
open Infrastructure.Types

open Conference.Types
open Conference.Ws

let updateStateWithEvents state events =
  match state with
  | Success state ->
       events
        |> List.fold Projections.apply state
        |> Success

  | _ -> state

let init() =
  {
    Info = "noch nicht connected"
    State = NotAsked
  }, Cmd.ofSub startWs


let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
  model, Cmd.none

