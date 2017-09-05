module App.Types

open Global

type Msg =
  | ConferenceMsg of Conference.Types.Msg
  | CounterMsg of Counter.Types.Msg
  | LoginMsg of Login.Types.Msg
  | LoggedIn
  | LoggedOut
  | StorageFailure of exn
  | Logout

type Model = {
    CurrentPage: Page
    CurrentUser : UserData option
    LoginModel: Login.Types.Model
    CounterModel: Counter.Types.Model
    ConferenceModel : Conference.Types.Model
  }
