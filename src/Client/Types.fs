module App.Types

open Global

type Msg =
  | ConferenceMsg of Conference.Types.Msg
  | LoginMsg of Login.Types.Msg
  | WsMsg of Ws.Msg
  | LoggedIn of UserData
  | LoggedOut
  | StorageFailure of exn
  | Logout

type CurrentPage =
  | HomePage
  | Login of Login.Types.Model
  | Conference of Conference.Types.Model
  | Ws of Ws.Model


type Model =
  {
    User : UserData option
    CurrentPage : CurrentPage
  }
