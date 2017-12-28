module Login.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Client.Style
open System
open Fable.Core.JsInterop

open Login.Types

let view model (dispatch: Msg -> unit) =
    let showErrorClass = if String.IsNullOrEmpty model.ErrorMsg then "hidden" else ""
    let buttonActive = if String.IsNullOrEmpty model.Login.UserName || String.IsNullOrEmpty model.Login.Password then "btn-disabled" else "btn-primary"

    match model.State with
    | LoggedIn user ->
        div [Id "greeting"] [
          h3 [ ClassName "text-center" ] [ str (sprintf "Hi %s!" user.UserName) ]
        ]

    | LoggedOut ->
        div [ClassName "signInBox" ] [
          h3 [ ClassName "text-center" ] [ str "Log in with 'test' / 'test'."]

          div [ ClassName showErrorClass ] [
                  div [ ClassName "alert alert-danger" ] [ str model.ErrorMsg ]
           ]

          div [ ClassName "input-group input-group-lg" ] [
                span [ClassName "input-group-addon" ] [
                  span [ClassName "glyphicon glyphicon-user"] []
                ]
                input [
                    Id "username"
                    HTMLAttr.Type "text"
                    ClassName "form-control input-lg"
                    Placeholder "Username"
                    DefaultValue model.Login.UserName
                    OnChange (fun ev -> dispatch (SetUserName !!ev.target?value))
                    AutoFocus true ]
          ]

          div [ ClassName "input-group input-group-lg" ] [
                span [ClassName "input-group-addon" ] [
                  span [ClassName "glyphicon glyphicon-asterisk"] []
                ]
                input [
                        Id "password"
                        Key ("password_" + model.Login.PasswordId.ToString())
                        HTMLAttr.Type "password"
                        ClassName "form-control input-lg"
                        Placeholder "Password"
                        DefaultValue model.Login.Password
                        OnChange (fun ev -> dispatch (SetPassword !!ev.target?value))
                        onEnter ClickLogIn dispatch  ]
            ]

          div [ ClassName "text-center" ] [
              button [ ClassName ("btn " + buttonActive); OnClick (fun _ -> dispatch ClickLogIn) ] [ str "Log In" ]
          ]
        ]
