module Navbar.View

open Fable.React
open Fable.React.Props
open Fable.FontAwesome
open Global
open App.Types
open Fulma

let private navbarEnd =
  Navbar.End.div []
    [
      Navbar.Item.div []
        [
          Field.div [ Field.IsGrouped ]
            [
              Control.p [ ]
                [
                  Button.a
                    [
                      Button.Props [ Href "https://github.com/rommsen/ConfPlanner" ]
                    ]
                    [
                      Icon.icon [ ] [ Fa.i [ Fa.Brand.Github ] [] ]
                      span [ ] [ str "Source" ]
                    ]
                ]
            ]
        ]
    ]

let private menuItem label page currentPage =
  let isActive =
    match currentPage with
    | CurrentPage.About when page = Page.About ->
        true

    | CurrentPage.Login _ when page = Page.Login ->
        true

    | CurrentPage.Conference _ when page = Page.Conference ->
        true

    | _ ->
        false

  Navbar.Item.a
    [
      Navbar.Item.IsActive isActive
      Navbar.Item.Props [ Href <| toHash page ]
    ]
    [
      str label
    ]

let private viewLoginLogout dispatch user currentPage =
  match user with
  | None ->
      menuItem "Login" Page.Login currentPage

  | Some user ->
      Navbar.Item.a
        [
          Navbar.Item.Props [ OnClick (fun _ -> Logout |> dispatch) ]
        ]
        [
          str <| "Logout " + user.UserName
        ]

let private navbarStart dispatch user currentPage =
  Navbar.Start.a []
    [
      menuItem "Conference" Page.Conference currentPage
      menuItem "About" Page.About currentPage
      viewLoginLogout dispatch user currentPage
    ]

let view dispatch user currentPage =
  div [ ClassName "navbar-bg" ]
    [
      Container.container [ Container.IsFluid ]
        [
          Navbar.navbar [ Navbar.Color IsPrimary ]
            [
              Navbar.Brand.div [ ]
                [
                  Navbar.Item.a [ Navbar.Item.Props [ Href "#" ] ]
                    [
                      Heading.p [ Heading.Is4 ]
                        [ str "SAFE-ConfPlanner" ]
                    ]
                ]
              Navbar.menu []
                [
                  navbarStart dispatch user currentPage
                  navbarEnd
                ]
            ]
        ]
    ]
