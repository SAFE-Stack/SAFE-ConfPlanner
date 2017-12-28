module App.State

open Elmish
open Elmish.Browser.Navigation
open Fable.Import
open Global
open Types

let dispose currentPage =
  match currentPage with
  | CurrentPage.Conference _ ->
      Conference.State.dispose ()
      |> Cmd.map ConferenceMsg

  | _ -> Cmd.none

let withAdditionalCmd cmd (model, cmds) =
  model, (Cmd.batch [cmds ; cmd])

let urlUpdate (result : Page option) model =
  match result with
  | None ->
      Browser.console.error("Error parsing url: " + Browser.window.location.href)
      ( model, Navigation.modifyUrl (toHash Page.About) )

  | Some Page.Login ->
      let m,cmd = Login.State.init model.User
      { model with CurrentPage = CurrentPage.Login m }, Cmd.map LoginMsg cmd

  | Some Page.Conference ->
      match model.User with
      | Some user ->
          let m,cmd = Conference.State.init user
          { model with CurrentPage = CurrentPage.Conference m }, Cmd.map ConferenceMsg cmd

      | None ->
          model, Cmd.ofMsg Logout

  | Some Page.About ->
      { model with CurrentPage = CurrentPage.HomePage }, Cmd.none

  |> withAdditionalCmd (dispose model.CurrentPage)

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

let update msg model =
  match msg, model.CurrentPage with
  | ConferenceMsg msg, CurrentPage.Conference submodel->
      let (conference, conferenceCmd) = Conference.State.update msg submodel
      { model with CurrentPage = CurrentPage.Conference conference }, Cmd.map ConferenceMsg conferenceCmd

  // | Page.Websockets, WsMsg msg ->
  //     let (ws, wsCmd) = Ws.update msg model.WsModel
  //     { model with WsModel = ws }, Cmd.map WsMsg wsCmd

  | LoginMsg msg, CurrentPage.Login submodel ->
      let onSuccess newUser =
        if model.User = Some newUser then
            Cmd.ofMsg (LoggedIn newUser)
        else
            saveUserCmd newUser

      let m,cmd = Login.State.update LoginMsg onSuccess msg submodel

      { model with CurrentPage = CurrentPage.Login m }, cmd

  | LoggedIn newUser, _->
      let nextPage = Page.Conference
      { model with User = Some newUser }, Navigation.newUrl (toHash nextPage)

  | LoggedOut, _ ->
      { model with
          User = None
          CurrentPage = CurrentPage.HomePage },
        Navigation.newUrl (toHash Page.About)

  | StorageFailure error, _ ->
      printfn "Unable to access local storage: %A" error
      model, Cmd.none

  | Logout, _ ->
      model, deleteUserCmd

  | _ , _ ->
      model, Cmd.none
