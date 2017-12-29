module Navbar.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma.Components
open Fulma.Elements
open Fulma.Layouts
open Fulma.Extra.FontAwesome
open Fulma.Elements.Form
open Global
open App.Types

let navbarEnd =
  Navbar.end_div []
    [
      Navbar.item_div []
        [
          Field.field_div [ Field.isGrouped ]
            [
              Control.control_p [ ]
                [
                  Button.button_a
                    [
                      Button.props [ Href "https://github.com/rommsen/ConfPlanner" ]
                    ]
                    [
                      Icon.faIcon [ ] [ Fa.icon Fa.I.Github ]
                      span [ ] [ str "Source" ]
                    ]
                ]
            ]
        ]
    ]

let menuItem label page currentPage =
  let isActive =
    match currentPage with
    | CurrentPage.HomePage when page = Page.About ->
        true

    | CurrentPage.Login _ when page = Page.Login ->
        true

    | CurrentPage.Conference _ when page = Page.Conference ->
        true

    | _ ->
        false

  Navbar.item_a
    [
      if isActive then
        yield Navbar.Item.isActive

      yield Navbar.Item.props [ Href <| toHash page ]
    ]
    [
      str label
    ]

let navbarStart currentPage =
  Navbar.start_div []
    [
      menuItem "Conference" Page.Conference currentPage
      // menuItem "Dummy" Page.Websockets currentPage
      menuItem "About" Page.About currentPage
    ]

let view currentPage =
  div [ ClassName "navbar-bg" ]
    [
      Container.container [ Container.isFluid ]
        [
          Navbar.navbar [ Navbar.isPrimary ]
            [
              Navbar.brand_div [ ]
                [
                  Navbar.item_a [ Navbar.Item.props [ Href "#" ] ]
                    [
                      Heading.p [ Heading.is4 ]
                        [ str "ConfPlanner" ]
                    ]
                ]
              Navbar.menu []
                [
                  navbarStart currentPage
                  navbarEnd
                ]
            ]
        ]
    ]
