module Login.State

open Fable.Core
open Fable.Import
open Elmish
open System
open Fable.Core.JsInterop
open Fable.Import.Browser
open Fable.PowerPack
open Fable.PowerPack.Fetch.Fetch_types

open Server.AuthTypes
open Login.Types
open Global

let authUser (login:Login,apiUrl) =
  promise {
    if String.IsNullOrEmpty login.UserName then return! failwithf "You need to fill in a username." else
    if String.IsNullOrEmpty login.Password then return! failwithf "You need to fill in a password." else

    let body = toJson login

    let props =
      [ RequestProperties.Method HttpMethod.POST
        Fetch.requestHeaders [
          HttpRequestHeaders.ContentType "application/json" ]
        RequestProperties.Body !^body ]

    try
      let! response = Fetch.fetch apiUrl props

      if not response.Ok then
        return! failwithf "Error: %d" response.Status
      else
        let! data = response.text()
        return data
    with
    | _ -> return! failwithf "Could not authenticate user."
  }

let authUserCmd login apiUrl =
  Cmd.ofPromise authUser (login,apiUrl) GetTokenSuccess AuthError

let init (user : UserData option) =
  match user with
  | None ->
      {
        Login = { UserName = ""; Password = ""}
        State = LoggedOut
        ErrorMsg = ""
      }, Cmd.none
  | Some user ->
      {
        Login = { UserName = user.UserName; Password = ""}
        State = LoggedIn user.Token
        ErrorMsg = ""
      }, Cmd.none

let logout =
  {
      Login = { UserName = ""; Password = ""}
      State = LoggedOut
      ErrorMsg = ""
  }

let update (msg:Msg) model : Model*Cmd<Msg> =
    match msg with
    | GetTokenSuccess token ->
        { model with State = LoggedIn token;  Login = { model.Login with Password = "" } }, Cmd.none

    | SetUserName name ->
        { model with Login = { model.Login with UserName = name }}, Cmd.none

    | SetPassword pw ->
        { model with Login = { model.Login with Password = pw }}, Cmd.none

    | ClickLogIn ->
        model, authUserCmd model.Login "/api/users/login"

    | AuthError exn ->
        { model with ErrorMsg = string (exn.Message) }, Cmd.none
