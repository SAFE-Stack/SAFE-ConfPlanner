module App.State

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Import.Browser
open Global
open Types

let pageParser: Parser<Page->Page,Page> =
  oneOf [
    map Websockets (s "websockets")
  ]

let urlUpdate (result: Option<Page>) model =
  match result with
  | None ->
        console.error("Error parsing url")
        model,Navigation.modifyUrl (toHash model.CurrentPage)

  | Some page ->
       { model with CurrentPage = page }, Cmd.none

let init result =
  let (ws, wsCmd) = Ws.init()
  let (model, cmd) =
    urlUpdate result
      {
        CurrentPage = Websockets
        WsModel = ws
      }

  let cmds =
    [
      cmd
      Cmd.map WsMsg wsCmd
    ]
  model, Cmd.batch cmds

let update msg model =
  match msg with
  | WsMsg msg ->
      let (ws, wsCmd) = Ws.update msg model.WsModel
      { model with WsModel = ws }, Cmd.map WsMsg wsCmd
