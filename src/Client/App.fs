module App.App

open Elmish
open Elmish.Navigation
open App.State

open Elmish.HMR
open Elmish.React
open Elmish.Debug

// App
Program.mkProgram init update App.View.view
|> Program.toNavigable Global.urlParser urlUpdate
|> Program.withReactBatched "elmish-app"
|> Program.withConsoleTrace
|> Program.withDebugger
|> Program.run
