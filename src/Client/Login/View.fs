module Login.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open System
open Fable.Core.JsInterop

open Login.Types

open Fulma.Layouts
open Fulma.Elements
open Fulma.Color
open Fulma.Size
open Fulma.Extra.FontAwesome
open Fulma.Elements.Form

let private typeAndIconAndError error =
  match error with
  | Some error ->
      let help =
        Help.help
          [ Help.Color IsDanger ]
          [ str error ]

      Input.Color IsDanger,Fa.I.Times,help

  | None ->
      Input.Color IsSuccess,Fa.I.Check,str ""

let private viewFormField typeIs changeMsg field error label props =
  let inputType,inputIcon,inputError =
    error |> typeAndIconAndError

  let defaultProps : IHTMLProp list =
    [
      OnChange (fun event -> !!event.target?value |>changeMsg)
    ]

  Form.Field.div []
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
          Icon.faIcon
            [
              Icon.Size IsSmall
              Icon.IsRight
            ]
            [ Fa.icon inputIcon ]

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
          Client.Utils.onEnter dispatch ClickLogIn
        ]

      viewFormField
        Input.Password
        (SetPassword>>dispatch)
        model.Login.Password
        model.ErrorMsg
        "Password (use: test)"
        [ Client.Utils.onEnter dispatch ClickLogIn ]
    ]

let private viewLoginRow content =
  Columns.columns []
    [
      Column.column
        [
          Column.Width (Column.All, Column.IsHalf)
          Column.Offset (Column.All, Column.IsOneThird)
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
