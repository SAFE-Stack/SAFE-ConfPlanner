module Global

open Elmish.Browser.UrlParser

type UserData =
  {
    UserName : string
    Token : Server.AuthTypes.JWT
  }

[<RequireQualifiedAccess>]
type Page =
  | Conference
  | About
  | Login
  | Websockets

let toHash page =
  match page with
  | Page.About -> "#about"
  | Page.Login -> "#login"
  | Page.Conference -> "#conference"
  | Page.Websockets -> "#websockets"

let private pageParser : Parser<Page -> Page,_> =
  oneOf
    [
      map Page.About (s "about")
      map Page.Login (s "login")
      map Page.Conference (s "conference")
      map Page.Websockets (s "websockets")
    ]

let urlParser location =
  parseHash pageParser location

type RemoteData<'Result> =
  | NotAsked
  | Loading
  | Failure of string
  | Success of 'Result
