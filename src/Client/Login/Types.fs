module Login.Types

open Server.AuthTypes

type LoginState =
| LoggedOut
| LoggedIn of JWT

type Model = {
    State : LoginState
    Login : Login
    ErrorMsg : string }

type Msg =
  | GetTokenSuccess of string
  | SetUserName of string
  | SetPassword of string
  | AuthError of exn
  | ClickLogIn
