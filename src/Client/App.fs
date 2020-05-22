module App.App

open Elmish
open Elmish.Navigation

open Elmish.React
open Elmish.Debug

open Thoth.Elmish

// App
Program.mkProgram State.init State.update View.view
|> Program.toNavigable Utils.Navigation.urlParser State.urlUpdate
// |> Program.withConsoleTrace
|> Toast.Program.withToast Toast.render
|> Program.withReactSynchronous "elmish-app"
// #if DEBUG
//  |> Program.withDebugger
// #endif
|> Program.run
