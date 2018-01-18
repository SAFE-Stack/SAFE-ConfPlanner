module Global

open Elmish.Browser.UrlParser
open Domain.Model
open Server.AuthTypes

type UserData =
  {
    UserName : Username
    Roles : Role list
    Token : Server.AuthTypes.JWT
  }

[<RequireQualifiedAccess>]
type Page =
  | Conference
  | About
  | Login

let toHash page =
  match page with
  | Page.About -> "#about"
  | Page.Login -> "#login"
  | Page.Conference -> "#conference"

let private pageParser : Parser<Page -> Page,_> =
  oneOf
    [
      map Page.About (s "about")
      map Page.Login (s "login")
      map Page.Conference (s "conference")
    ]

let urlParser location =
  parseHash pageParser location

type RemoteData<'Result> =
  | NotAsked
  | Loading
  | Failure of string
  | Success of 'Result

let LocalStorageUserKey = "ConfPlanner-User"
