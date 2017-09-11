module Global

open Server.AuthTypes

type UserData =
  { UserName : string
    Token : JWT }

type Page =
  | Conference
  | Counter
  | About
  | Login
  | Websockets


let toHash page =
  match page with
  | About -> "#about"
  | Counter -> "#counter"
  | Login -> "#login"
  | Conference -> "#conference"
  | Websockets -> "#websockets"
