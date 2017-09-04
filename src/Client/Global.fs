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

let toHash page =
  match page with
  | About -> "#about"
  | Counter -> "#counter"
  | Login -> "#login"
  | Conference -> "#conference"
