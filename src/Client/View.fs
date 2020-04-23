module App.View

open Fable.Core.JsInterop
open App.Types

open Fable.React

let view model dispatch =
  let pageHtml currentPage =
    match currentPage with
    | CurrentPage.About ->
        Info.View.view

    | CurrentPage.Login submodel ->
        Login.View.view (LoginMsg >> dispatch) submodel

    | CurrentPage.Conference submodel ->
        Conference.View.view (ConferenceMsg >> dispatch) submodel

  [
    Navbar.View.view dispatch model.User model.CurrentPage
    pageHtml model.CurrentPage
  ]
  |> div []

