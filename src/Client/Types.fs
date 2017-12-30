module App.Types

open Global

type Msg =
  | ConferenceMsg of Conference.Types.Msg
  | LoginMsg of Login.Types.Msg
  | LoggedIn of UserData
  | LoggedOut
  | StorageFailure of exn
  | Logout

type CurrentPage =
  | About
  | Login of Login.Types.Model
  | Conference of Conference.Types.Model

type Model =
  {
    User : UserData option
    CurrentPage : CurrentPage
  }
