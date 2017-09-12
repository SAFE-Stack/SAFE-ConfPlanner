module App.View

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser
open App.Types
open Global

importAll "../../sass/main.sass"

open Fable.Helpers.React
open Fable.Helpers.React.Props

let menuItem label page currentPage =
    li
      [ ]
      [ a
          [ classList [ "is-active", page = currentPage ]
            Href (toHash page) ]
          [ str label ] ]

let menu currentPage =
  aside
    [ ClassName "menu" ]
    [ p
        [ ClassName "menu-label" ]
        [ str "General" ]
      ul
        [ ClassName "menu-list" ]
        [
          menuItem "Conference" Page.Conference currentPage
          menuItem "Counter" Page.Counter currentPage
          menuItem "About" Page.About currentPage
          menuItem "Websockets" Page.Websockets currentPage
        ]
    ]

let view model dispatch =

  let pageHtml =
    function
    | Page.About -> Info.View.root
    | Counter -> Counter.View.root model.CounterModel (CounterMsg >> dispatch)
    | Login -> Login.View.root model.LoginModel (LoginMsg >> dispatch)
    | Conference -> Conference.View.root model.ConferenceModel (ConferenceMsg >> dispatch)
    | Websockets -> Ws.root model.WsModel (WsMsg >> dispatch)

  div
    []
    [ div
        [ ClassName "navbar-bg" ]
        [ div
            [ ClassName "container" ]
            [ Navbar.View.root model.CurrentUser (fun _ -> Logout |> dispatch) ] ]
      div
        [ ClassName "section" ]
        [ div
            [ ClassName "container" ]
            [ div
                [ ClassName "columns" ]
                [ div
                    [ ClassName "column is-3" ]
                    [ menu model.CurrentPage ]
                  div
                    [ ClassName "column" ]
                    [ pageHtml model.CurrentPage ] ] ] ] ]
