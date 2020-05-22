module Elmish.Helper

open Elmish

let withAdditionalCommand cmd (model, cmds) =
  model, (Cmd.batch [cmds ; cmd])


let withCommand (cmds : Cmd<'a>) model =
  model, cmds

let withoutCmds model =
  model, Cmd.none



type Deferred<'t> =
  | HasNotStartedYet
  | InProgress
  | Resolved of 't


type AsyncOperationStatus<'t> =
  | Started
  | Finished of 't

module Cmd =
  let fromAsync (operation: Async<'msg>) : Cmd<'msg> =
    let delayedCmd (dispatch: 'msg -> unit) : unit =
      let delayedDispatch = async {
          let! msg = operation
          dispatch msg
      }

      Async.StartImmediate delayedDispatch

    Cmd.ofSub delayedCmd
