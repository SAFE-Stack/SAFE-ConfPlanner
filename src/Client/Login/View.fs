module Login.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open System
open Fable.Core.JsInterop

open Login.Types

open Fulma.Layouts
open Fulma.Elements
open Fulma.Extra.FontAwesome
open Fulma.Elements.Form
open Server.AuthTypes

let private typeAndIconAndError error =
  match error with
  | Some error ->
      let help =
        Help.help
          [ Help.isDanger ]
          [ str error ]

      Input.isDanger,Fa.I.Times,help

  | None ->
      Input.isSuccess,Fa.I.Check,str ""

let private viewFormField typeIs changeMsg field error label props =
  let inputType,inputIcon,inputError =
    error |> typeAndIconAndError

  let defaultProps : IHTMLProp list =
    [
      OnChange (fun event -> !!event.target?value |>changeMsg)
    ]

  Form.Field.field_div []
    [
      Label.label [] [ str label ]
      Control.control_div
        [
           Control.hasIconRight
        ]
        [
          Input.input
            [
              inputType
              typeIs
              Input.placeholder label
              Input.value field
              Input.props <| List.concat [ defaultProps ; props ]

            ]
          Icon.faIcon
            [
              Icon.isSmall
              Icon.isRight
            ]
            [ Fa.icon inputIcon ]

        ]
      inputError
    ]

let private viewForm dispatch model =
  let (Username username) = model.Login.UserName
  let (Password password) = model.Login.Password
  form [ ]
    [
      viewFormField
        Input.typeIsText
        (SetUserName>>dispatch)
        username
        model.ErrorMsg
        "Username (use: test)"
        [
          AutoFocus true
          Client.Utils.onEnter dispatch ClickLogIn
        ]

      viewFormField
        Input.typeIsPassword
        (SetPassword>>dispatch)
        password
        model.ErrorMsg
        "Password (use: test)"
        [ Client.Utils.onEnter dispatch ClickLogIn ]
    ]

let private viewLoginRow content =
  Columns.columns []
    [
      Column.column
        [
          Column.Width.isHalf
          Column.Offset.isOneThird
        ]
        [
          content
        ]
    ]

let private credentialsValid (Username username) (Password password) =
  String.IsNullOrEmpty username || String.IsNullOrEmpty password

let private viewLoginButton dispatch username password =
  Button.button_a
    [
      yield Button.onClick (fun _ -> ClickLogIn |> dispatch)
      yield Button.isPrimary
      if credentialsValid username password then
        yield Button.isDisabled
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
