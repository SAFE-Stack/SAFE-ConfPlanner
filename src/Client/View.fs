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
    li [ classList [ "is-active", page = currentPage ] ]
      [
        a [ Href <| toHash page ] [ str label ]
      ]

let tabs currentPage =
  div [ClassName "tabs is-centered"]
    [
      ul []
        [
          menuItem "Websockets" Page.Websockets currentPage
        ]
    ]

let view model dispatch =

  let pageHtml =
    function
    | Websockets -> Ws.root model.WsModel (WsMsg >> dispatch)

  div []
    [
      div [ ClassName "navbar-bg" ]
        [ div
            [ ClassName "container" ]
            [ Navbar.View.root () ] ]

      tabs model.CurrentPage
      pageHtml model.CurrentPage
    ]

