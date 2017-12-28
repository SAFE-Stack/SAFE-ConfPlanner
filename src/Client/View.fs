module App.View

open Fable.Core.JsInterop
open App.Types

importAll "../../sass/main.sass"

open Fable.Helpers.React

let view model dispatch =
  let pageHtml currentPage =
    match currentPage with
    | CurrentPage.HomePage ->
        Info.View.view

    | CurrentPage.Login submodel ->
        Login.View.view submodel (LoginMsg >> dispatch)

    | CurrentPage.Conference submodel ->
        Conference.View.view submodel (ConferenceMsg >> dispatch)

    | CurrentPage.Ws submodel ->
        Ws.view submodel (WsMsg >> dispatch)

  [
    Navbar.View.view model.CurrentPage
    pageHtml model.CurrentPage
  ]
  |> div []

