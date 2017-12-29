module Login.State

open Elmish
open Elmish.Helper
open System
open Fable.Core.JsInterop
open Fable.PowerPack
open Fable.PowerPack.Fetch.Fetch_types

open Server.AuthTypes
open Login.Types
open Global
open Client
open Server.ServerTypes

let private authUser (login:Login) =
  promise {
    if String.IsNullOrEmpty login.UserName then return! failwithf "You need to fill in a username." else
    if String.IsNullOrEmpty login.Password then return! failwithf "You need to fill in a password." else

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
          |> ofJson<UserRights>

        return
            {
              OrganizerId = userRights.OrganizerId
              UserName = userRights.UserName
              Token = data
            }
    with
    | _ -> return! failwithf "Could not authenticate user."
  }

let private authUserCmd login =
  Cmd.ofPromise authUser login LoginSuccess AuthError

let private withStateLoggedIn user model =
  { model with State = LoggedIn user }

let init (user : UserData option) =
  let model =
     {
       Login = { UserName = ""; Password = ""; PasswordId = Guid.NewGuid() }
       State = LoggedOut
       ErrorMsg = ""
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

let update f onSuccess (msg:Msg) model : Model*Cmd<'a> =
  match msg with
  | LoginSuccess user ->
      { model with State = LoggedIn user }
      |> withLogin { model.Login with Password = ""; PasswordId = Guid.NewGuid() }
      |> withCommand (onSuccess user)

  | SetUserName name ->
      model
      |> withLogin { model.Login with UserName = name; Password = ""; PasswordId = Guid.NewGuid() }
      |> withoutCommands

  | SetPassword pw ->
      model
      |> withLogin { model.Login with Password = pw }
      |> withoutCommands

  | ClickLogIn ->
      model
      |> withCommand (authUserCmd model.Login |> Cmd.map f)

  | AuthError exn ->
      { model with ErrorMsg = string (exn.Message) }
      |> withoutCommands
