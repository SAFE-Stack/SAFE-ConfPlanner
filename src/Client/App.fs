module App.App

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open App.State

open Elmish.HMR
open Elmish.React
open Elmish.Debug

// App
Program.mkProgram init update App.View.view
|> Program.toNavigable (parseHash pageParser) urlUpdate
|> Program.withHMR
|> Program.withReact "elmish-app"
|> Program.withConsoleTrace
|> Program.withDebugger
|> Program.run
