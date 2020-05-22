module Login.View

open Fable.React
open Fable.React.Props
open System
open Fable.Core.JsInterop
open Fable.FontAwesome

open Login.Types

open Fulma

let private typeAndIconAndError error =
  match error with
  | Some error ->
      let help =
        Help.help
          [ Help.Color IsDanger ]
          [ str error ]

      Input.Color IsDanger,Fa.Solid.Times,help

  | None ->
      Input.Color IsSuccess,Fa.Solid.Check,str ""

let private viewFormField typeIs changeMsg field error label props =
  let inputType,inputIcon,inputError =
    error |> typeAndIconAndError

  let defaultProps : IHTMLProp list =
    [
      OnChange (fun event -> !!event.target?value |>changeMsg)
    ]

  Field.div []
    [
      Label.label [] [ str label ]
      Control.div
        [
           Control.HasIconRight
        ]
        [
          Input.input
            [
              inputType
              Input.Type typeIs
              Input.Placeholder label
              Input.Value field
              Input.Props <| List.concat [ defaultProps ; props ]

            ]
          Icon.icon
            [
              Icon.Size IsSmall
              Icon.IsRight
            ]
            [ Fa.i [ inputIcon ] [] ]

        ]
      inputError
    ]

let private viewForm dispatch model =
  form [ ]
    [
      viewFormField
        Input.Text
        (SetUserName>>dispatch)
        model.Login.UserName
        model.ErrorMsg
        "Username (use: test)"
        [
          AutoFocus true
          Utils.JS.onEnter dispatch ClickLogIn
        ]

      viewFormField
        Input.Password
        (SetPassword>>dispatch)
        model.Login.Password
        model.ErrorMsg
        "Password (use: test)"
        [ Utils.JS.onEnter dispatch ClickLogIn ]
    ]

let private viewLoginRow content =
  Columns.columns []
    [
      Column.column
        [
          Column.Width (Screen.All, Column.IsHalf)
          Column.Offset (Screen.All, Column.IsOneThird)
        ]
        [
          content
        ]
    ]

let private viewLoginButton dispatch username password =
  Button.a
    [
      Button.OnClick (fun _ -> ClickLogIn |> dispatch)
      Button.Color IsPrimary
      Button.Disabled (String.IsNullOrEmpty username || String.IsNullOrEmpty password)
    ]
    [ str "Log in" ]


let private viewLoginForm dispatch model =
  [
    yield viewLoginRow (viewForm dispatch model)
    yield viewLoginRow (viewLoginButton dispatch model.Login.UserName model.Login.Password)
  ]
  |> Section.section []

let view dispatch model =
  match model.State with
  | LoggedOut ->
    viewLoginForm dispatch model

  | LoggedIn _ ->
      str ""
