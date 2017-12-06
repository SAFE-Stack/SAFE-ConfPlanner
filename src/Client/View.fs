module App.View

open Fable.Core.JsInterop
open App.Types
open Global

importAll "../../sass/main.sass"

open Fable.Helpers.React

let view model dispatch =

  let pageHtml =
    function
    | Page.About -> Info.View.view
    | Counter -> Counter.View.root model.CounterModel (CounterMsg >> dispatch)
    | Login -> Login.View.root model.LoginModel (LoginMsg >> dispatch)
    | ConfPlanner -> Conference.View.root model.ConferenceModel (ConferenceMsg >> dispatch)
    | Websockets -> Ws.root model.WsModel (WsMsg >> dispatch)

  div []
    [
      Navbar.View.view model.CurrentPage
      pageHtml model.CurrentPage
    ]

