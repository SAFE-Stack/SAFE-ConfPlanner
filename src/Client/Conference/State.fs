module Conference.State

open Elmish
open Conference.Types

let init () : Model * Cmd<Msg> =
  { Something = "hallo" }, Cmd.none

let update msg model =
  match msg with
  | _ ->
      model, Cmd.none
