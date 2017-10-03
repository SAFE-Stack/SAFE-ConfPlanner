module App.App

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser
open App.Types
open App.State

open Elmish.HMR
open Elmish.React
open Elmish.Debug

// App
Program.mkProgram init update App.View.view
|> Program.toNavigable (parseHash pageParser) urlUpdate
|> Program.withHMR
|> Program.withReact "elmish-app"
|> Program.withDebugger
|> Program.run
