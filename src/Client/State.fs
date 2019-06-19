module App.State

open Elmish
open Elmish.Navigation
open Elmish.Helper
open Fable.Import
open Global
open Types

open Thoth.Json
open Fable.Core

let private disposeCmd currentPage =
  match currentPage with
  | CurrentPage.Conference _ ->
      Conference.State.dispose ()
      |> Cmd.map ConferenceMsg

  | _ -> Cmd.none

let private withCurrentPage page model =
   { model with CurrentPage = page }

let private navigateTo page model =
  model
  |> withCommand (page |> toHash |> Navigation.newUrl)

let urlUpdate (result : Page option) model =
  match result with
  | None ->
      JS.console.error("Error parsing url: " + Browser.Dom.window.location.href)
      model
      |> navigateTo Page.About

  | Some Page.Login ->
      let m,cmd = Login.State.init model.User
      { model with CurrentPage = CurrentPage.Login m }
      |> withCommand (Cmd.map LoginMsg cmd)

  | Some Page.Conference ->
      match model.User with
      | Some user ->
          let submodel,cmd = Conference.State.init user
          { model with CurrentPage = CurrentPage.Conference submodel }
          |> withCommand (Cmd.map ConferenceMsg cmd)

      | None ->
          model |> navigateTo Page.Login

  | Some Page.About ->
      { model with CurrentPage = CurrentPage.About }
      |> withoutCommands

  |> withAdditionalCommand (disposeCmd model.CurrentPage)

let loadUser () : UserData option =
  let userDecoder = Decode.Auto.generateDecoder<UserData>()
  match LocalStorage.load userDecoder LocalStorageUserKey with
  | Ok user -> Some user
  | Error _ -> None

let private saveUserCmd user =
  Cmd.OfFunc.either (LocalStorage.save LocalStorageUserKey) user (fun _ -> LoggedIn user) StorageFailure

let private deleteUserCmd =
  Cmd.OfFunc.either LocalStorage.delete LocalStorageUserKey (fun _ -> LoggedOut) StorageFailure

let init result =
  let user : UserData option = loadUser ()
  let model =
    {
      User = user
      CurrentPage = About
    }
  urlUpdate result model

let update msg model =
  match msg, model.CurrentPage with
  | ConferenceMsg msg, CurrentPage.Conference submodel->
      let (conference, conferenceCmd) = Conference.State.update msg submodel
      model
      |> withCurrentPage (CurrentPage.Conference conference )
      |> withCommand (Cmd.map ConferenceMsg conferenceCmd)

  | LoginMsg msg, CurrentPage.Login submodel ->
      let onSuccess newUser =
        if model.User = Some newUser then
            Cmd.ofMsg <| LoggedIn newUser
        else
            saveUserCmd newUser

      let submodel,cmd = Login.State.update LoginMsg onSuccess msg submodel

      model
      |> withCurrentPage (CurrentPage.Login submodel)
      |> withCommand cmd

  | LoggedIn newUser, _->
      { model with User = Some newUser }
      |> navigateTo Page.Conference

  | LoggedOut, _ ->
      { model with User = None }
      |> withCurrentPage CurrentPage.About
      |> navigateTo Page.About

  | StorageFailure error, _ ->
      printfn "Unable to access local storage: %A" error
      model |> withoutCommands

  | Logout, _ ->
      model |> withCommand deleteUserCmd

  | _ , _ ->
      model |> withoutCommands
