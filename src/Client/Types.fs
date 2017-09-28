module App.Types

open Global

type Msg =
  | ConferenceMsg of Conference.View.Msg
  | CounterMsg of Counter.Types.Msg
  | LoginMsg of Login.Types.Msg
  | WsMsg of Ws.Msg
  | LoggedIn
  | LoggedOut
  | StorageFailure of exn
  | Logout

type Model = {
    CurrentPage: Page
    CurrentUser : UserData option
    LoginModel: Login.Types.Model
    CounterModel: Counter.Types.Model
    ConferenceModel : Conference.View.Model
    WsModel : Ws.Model
  }
