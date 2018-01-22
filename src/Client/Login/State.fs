module Login.State

open Elmish
open Elmish.Helper
open System
open Fable.Core.JsInterop
open Fable.PowerPack
open Fable.PowerPack.Fetch.Fetch_types

open Infrastructure.Auth
open Login.Types
open Global
open Client
open Server.ServerTypes
open Domain.Model

let private authUser (login:Login) =
  promise {
    let (Username username) = login.UserName
    let (Password password) = login.Password
    if String.IsNullOrEmpty username then return! failwithf "You need to fill in a username." else
    if String.IsNullOrEmpty password then return! failwithf "You need to fill in a password." else

    let body = toJson login

    let props =
      [
        RequestProperties.Method HttpMethod.POST
        Fetch.requestHeaders [
          HttpRequestHeaders.ContentType "application/json" ]
        RequestProperties.Body !^body
      ]

    try
      let! response = Fetch.fetch Server.Urls.Login props

      if not response.Ok then
        return! failwithf "Error: %d" response.Status
      else
        let! data = response.text()

        let userRights =
          data
          |> Utils.decodeJwt
          |> ofJson<UserRights<User,Roles.Container>>

        return
            {
              Username = userRights.Username
              User = userRights.User
              Roles = userRights.Permissions
              Token = data
            }
    with
    | _ -> return! failwithf "Could not authenticate user (did you run the fixtures for demo data? See README in Repo)."
  }

let private authUserCmd login =
  Cmd.ofPromise authUser login LoginSuccess AuthError

let private withStateLoggedIn user model =
  { model with State = LoggedIn user }

let init (user : UserData option) =
  let model =
     {
       Login = { UserName = Username ""; Password = Password ""; PasswordId = Guid.NewGuid() }
       State = LoggedOut
       ErrorMsg = None
      }

  match user with
  | None ->
     model |> withoutCommands

  | Some user ->
      model
      |> withStateLoggedIn user
      |> withoutCommands

let private withLogin login model =
  { model with Login = login }

let withError error model =
  { model with ErrorMsg = error |> Some }

let withoutError model =
  { model with ErrorMsg = None }

let update f onSuccess (msg:Msg) model : Model*Cmd<'a> =
  match msg with
  | LoginSuccess user ->
      { model with State = LoggedIn user }
      |> withLogin { model.Login with Password = Password ""; PasswordId = Guid.NewGuid() }
      |> withCommand (onSuccess user)

  | SetUserName name ->
      model
      |> withLogin { model.Login with UserName = Username name; Password = Password ""; PasswordId = Guid.NewGuid() }
      |> withoutError
      |> withoutCommands

  | SetPassword password ->
      model
      |> withoutError
      |> withLogin { model.Login with Password = Password password }
      |> withoutCommands

  | ClickLogIn ->
      model
      |> withoutError
      |> withCommand (authUserCmd model.Login |> Cmd.map f)

  | AuthError exn ->
      model
      |> withError (exn.Message |> string)
      |> withoutCommands
