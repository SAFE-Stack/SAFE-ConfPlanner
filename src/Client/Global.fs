module Global

open Server.AuthTypes

type UserData =
  { UserName : string
    Token : JWT }

type Page =
  | ConfPlanner
  | Counter
  | About
  | Login
  | Websockets


let toHash page =
  match page with
  | About -> "#about"
  | Counter -> "#counter"
  | Login -> "#login"
  | ConfPlanner -> "#conference"
  | Websockets -> "#websockets"

type RemoteData<'Result> =
  | NotAsked
  | Loading
  | Failure of string
  | Success of 'Result
