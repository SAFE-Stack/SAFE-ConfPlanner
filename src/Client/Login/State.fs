module Login.State

open Elmish
open System
open Fable.Core.JsInterop
open Fable.PowerPack
open Fable.PowerPack.Fetch.Fetch_types

open Server.AuthTypes
open Login.Types
open Global
open Client
open Server.ServerTypes

let authUser (login:Login) =
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

        printfn "rights %A" userRights
        return
            {
              OrganizerId = userRights.OrganizerId
              UserName = userRights.UserName
              Token = data
            }
    with
    | _ -> return! failwithf "Could not authenticate user."
  }

let authUserCmd login =
  Cmd.ofPromise authUser login LoginSuccess AuthError

let init (user:UserData option) =
  match user with
  | None ->
      { Login = { UserName = ""; Password = ""; PasswordId = Guid.NewGuid() }
        State = LoggedOut
        ErrorMsg = "" }, Cmd.none
  | Some user ->
      { Login = { UserName = user.UserName; Password = ""; PasswordId = Guid.NewGuid() }
        State = LoggedIn user
        ErrorMsg = "" }, Cmd.none

let update f onSuccess (msg:Msg) model : Model*Cmd<'a> =
  match msg with
  | LoginSuccess user ->
      { model with State = LoggedIn user; Login = { model.Login with Password = ""; PasswordId = Guid.NewGuid() } }, onSuccess user
  | SetUserName name ->
      { model with Login = { model.Login with UserName = name; Password = ""; PasswordId = Guid.NewGuid() } }, Cmd.none
  | SetPassword pw ->
      { model with Login = { model.Login with Password = pw }}, Cmd.none
  | ClickLogIn ->
      model, authUserCmd model.Login |> Cmd.map f
  | AuthError exn ->
      { model with ErrorMsg = string (exn.Message) }, Cmd.none
