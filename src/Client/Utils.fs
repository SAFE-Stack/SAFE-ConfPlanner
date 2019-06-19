module Client.Utils

open Fable.Import
open Fable.Core
open Fable.React.Props

[<Emit("atob($0)")>]
let atob (str : string) : string = jsNative

let decodeJwt (jwt : string) =
  (jwt.Split ".").[1]
  |> atob


let onEnter dispatch msg =
  OnKeyDown (fun (ev:Browser.Types.KeyboardEvent) ->
      match ev with
      | _ when ev.keyCode = 13. ->
          ev.preventDefault()
          dispatch msg

      | _ -> ())

