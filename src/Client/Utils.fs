namespace Utils

module Navigation =
  open Elmish.UrlParser

  [<RequireQualifiedAccess>]
  type Page =
    | Conference
    | About
    | Login

  let toHash page =
    match page with
    | Page.About -> "#about"
    | Page.Login -> "#login"
    | Page.Conference -> "#conference"

  let private pageParser : Parser<Page -> Page,_> =
    oneOf
      [
        map Page.About (s "about")
        map Page.Login (s "login")
        map Page.Conference (s "conference")
      ]

  let urlParser location =
    parseHash pageParser location

module JS =

  open Fable.Import
  open Fable.Core
  open Fable.React.Props
  [<Emit("atob($0)")>]
  let atob (str : string) : string = jsNative

  let decodeJwt (jwt : string) =
    jwt.Split('.').[1]
    |> atob


  let onEnter dispatch msg =
    OnKeyDown (fun (ev:Browser.Types.KeyboardEvent) ->
        match ev with
        | _ when ev.keyCode = 13. ->
            ev.preventDefault()
            dispatch msg

        | _ -> ())

module Elmish =

  open Elmish

  type Deferred<'t> =
    | HasNotStartedYet
    | InProgress
    | Resolved of 't


  type AsyncOperationStatus<'t> =
    | Started
    | Finished of 't

  let withAdditionalCmd cmd (model, cmds) =
    model, (Cmd.batch [cmds ; cmd])

  let withCmd (cmds : Cmd<'a>) model =
    model, cmds

  let withoutCmds model =
    model, Cmd.none

module Cmd =
  open Elmish

  let fromAsync (operation: Async<'msg>) : Cmd<'msg> =
    let delayedCmd (dispatch: 'msg -> unit) : unit =
      let delayedDispatch = async {
          let! msg = operation
          dispatch msg
      }

      Async.StartImmediate delayedDispatch

    Cmd.ofSub delayedCmd


  let  msgSendAfterMilliseconds timeout msg  =
    fun dispatch -> Browser.Dom.window.setTimeout((fun _ -> msg |> dispatch), timeout) |> ignore
    |> Cmd.ofSub

