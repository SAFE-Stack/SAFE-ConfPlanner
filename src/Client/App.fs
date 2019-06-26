module App.App

open Elmish
open Elmish.Navigation

open Elmish.React
open Elmish.Debug
open Elmish.Streams
open Types

let asConferenceMsg = function
  | ConferenceMsg msg -> Some msg
  | _ -> None

let stream model msgs =
  match model.CurrentPage with
  | Conference model ->
      msgs
      |> Stream.subStream Conference.State.stream model asConferenceMsg ConferenceMsg "conference"

  | _ -> msgs

// App
Program.mkProgram State.init State.update View.view
|> Program.withStream stream "stream"
|> Program.toNavigable Global.urlParser State.urlUpdate
|> Program.withConsoleTrace
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
