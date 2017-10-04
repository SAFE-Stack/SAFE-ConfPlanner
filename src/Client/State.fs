module App.State

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Import.Browser
open Global
open Types

let pageParser: Parser<Page->Page,Page> =
  oneOf [
    map About (s "about")
    map Counter (s "counter")
    map Login (s "login")
    map ConfPlanner (s "conference")
    map Websockets (s "websockets")
  ]

let requiresAuthentication page model =
    match model.CurrentUser with
      | Some _ ->
          { model with CurrentPage = page }, Cmd.none

      | None ->
          { model with CurrentPage = Login }, Navigation.modifyUrl (toHash Login)

let urlUpdate (result: Option<Page>) model =
  match result with
  | None ->
        console.error("Error parsing url")
        model,Navigation.modifyUrl (toHash model.CurrentPage)

  | Some (Page.Counter as page) ->
      requiresAuthentication page model

  | Some page ->
       { model with CurrentPage = page }, Cmd.none

let init result =
  let user : UserData option = Client.Utils.load "user"
  let (conference, conferenceCmd) = Conference.State.init()
  let (counter, counterCmd) = Counter.State.init()
  let (login, loginCmd) = Login.State.init user
  let (ws, wsCmd) = Ws.init()
  let (model, cmd) =
    urlUpdate result
      {
        CurrentPage = About
        CurrentUser = user
        LoginModel = login
        CounterModel = counter
        ConferenceModel = conference
        WsModel = ws
      }

  let cmds =
    [
      cmd
      Cmd.map ConferenceMsg conferenceCmd
      Cmd.map CounterMsg counterCmd
      Cmd.map LoginMsg loginCmd
      Cmd.map WsMsg wsCmd
    ]
  model, Cmd.batch cmds

let update msg model =
  match msg with
  | ConferenceMsg msg ->
      let (conference, conferenceCmd) = Conference.State.update msg model.ConferenceModel
      { model with ConferenceModel = conference }, Cmd.map ConferenceMsg conferenceCmd

  | CounterMsg msg ->
      let (counter, counterCmd) = Counter.State.update msg model.CounterModel
      { model with CounterModel = counter }, Cmd.map CounterMsg counterCmd

  | WsMsg msg ->
      let (ws, wsCmd) = Ws.update msg model.WsModel
      { model with WsModel = ws }, Cmd.map WsMsg wsCmd

  | LoginMsg msg ->
      let loginModel, cmd = Login.State.update msg model.LoginModel
      let cmd = Cmd.map LoginMsg cmd

      match loginModel.State with
      | Login.Types.LoggedIn token ->
          let newUser : UserData = { UserName = loginModel.Login.UserName; Token = token }
          let cmd =
            if model.CurrentUser = Some newUser then
              cmd
            else
              Cmd.batch
                [
                  cmd
                  Cmd.ofFunc (Client.Utils.save "user") newUser (fun _ -> LoggedIn) StorageFailure
                ]

          { model with CurrentUser = Some newUser; LoginModel = loginModel }, cmd

      | Login.Types.LoggedOut ->
           { model with CurrentUser = None; LoginModel = loginModel }, cmd

  | LoggedIn ->
      model,
      Navigation.newUrl (toHash Page.Counter)

  | LoggedOut ->
      { model with CurrentUser = None; LoginModel = Login.State.logout },
      Navigation.newUrl (toHash Page.About)

  | StorageFailure error ->
      printfn "Unable to access local storage: %A" error
      model, Cmd.none

  | Logout ->
      model, Cmd.ofFunc Client.Utils.delete "user" (fun _ -> LoggedOut) StorageFailure
