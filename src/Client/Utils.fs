module Client.Utils

open Fable.Import
open Fable.Core

let load<'T> key =
    Browser.localStorage.getItem(key) |> unbox
    |> Option.map (JS.JSON.parse >> unbox<'T>)

let save key (data: 'T) =
    Browser.localStorage.setItem(key, JS.JSON.stringify data)

let delete key =
    Browser.localStorage.removeItem(key)


[<Emit("atob($0)")>]
let atob (str : string) : string = jsNative

let decodeJwt (jwt : string) =
  (jwt.Split ".").[1]
  |> atob

