module App.State

open Elmish
open Elmish.Browser.Navigation
open Elmish.Helper
open Fable.Import
open Global
open Types

let private disposeCmd currentPage =
  match currentPage with
  | CurrentPage.Conference _ ->
      Conference.State.dispose ()
      |> Cmd.map ConferenceMsg

  | _ -> Cmd.none

let urlUpdate (result : Page option) model =
  match result with
  | None ->
      Browser.console.error("Error parsing url: " + Browser.window.location.href)
      model, Navigation.modifyUrl (toHash Page.About)

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
          model, Navigation.newUrl (toHash Page.Login)

  | Some Page.About ->
      { model with CurrentPage = CurrentPage.HomePage }
      |> withoutCommands

  |> withAdditionalCommand (disposeCmd model.CurrentPage)

let private loadUser () =
  Client.Utils.load "user"

let saveUserCmd user =
    Cmd.ofFunc (Client.Utils.save "user") user (fun _ -> LoggedIn user) StorageFailure

let deleteUserCmd =
    Cmd.ofFunc Client.Utils.delete "user" (fun _ -> LoggedOut) StorageFailure

let init result =
  let user : UserData option = loadUser ()
  let model =
    {
      User = user
      CurrentPage = HomePage
    }
  urlUpdate result model

let private withCurrentPage page model =
   { model with CurrentPage = page }

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
      |> withCommand (Navigation.newUrl (toHash Page.Conference))

  | LoggedOut, _ ->
      { model with User = None }
      |> withCurrentPage CurrentPage.HomePage
      |> withCommand (Navigation.newUrl (toHash Page.About))

  | StorageFailure error, _ ->
      printfn "Unable to access local storage: %A" error
      model |> withoutCommands

  | Logout, _ ->
      model |> withCommand deleteUserCmd

  | _ , _ ->
      model |> withoutCommands
