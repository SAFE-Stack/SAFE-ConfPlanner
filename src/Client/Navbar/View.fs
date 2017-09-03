module Navbar.View

open Fable.Helpers.React
open Fable.Helpers.React.Props

open Global

let navButton classy href faClass txt =
    p
        [ ClassName "control" ]
        [ a
            [ ClassName (sprintf "button %s" classy)
              Href href ]
            [ span
                [ ClassName "icon" ]
                [ i
                    [ ClassName (sprintf "fa %s" faClass) ]
                    [ ] ]
              span
                [ ]
                [ str txt ] ] ]

let navButtons logout =
    span
        [ ClassName "nav-item" ]
        [ div
            [ ClassName "field is-grouped" ]
            [ navButton "twitter" "https://twitter.com/FableCompiler" "fa-twitter" "Twitter"
              navButton "github" "https://github.com/fable-compiler/fable-elmish" "fa-github" "Fork me"
              navButton "github" "https://gitter.im/fable-compiler/Fable" "fa-comments" "Gitter" ]
          span [ OnClick logout ] [ str "Logout"] ]

let root user logout =
  let info =
    match user with
    | Some user ->
        str (sprintf "Hi %s!" user.UserName)

    | None ->
        str "Not Logged In"

  nav
      [ ClassName "nav" ]
      [ div
          [ ClassName "nav-left" ]
          [ h1
              [ ClassName "nav-item is-brand title is-4" ]
              [ str "Elmish / " ; info ] ]
        div
          [ ClassName "nav-right" ]
          [ navButtons logout ] ]
