module Global

open Server.AuthTypes

type Page =
  | Counter
  | About
  | Login

type UserData =
  { UserName : string
    Token : JWT }

let toHash page =
  match page with
  | About -> "#about"
  | Counter -> "#counter"
  | Login -> "#login"
