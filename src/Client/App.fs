module App.App

open Elmish
open Elmish.Navigation

open Elmish.React
open Elmish.Debug

// App
Program.mkProgram State.init State.update View.view
|> Program.toNavigable Global.urlParser State.urlUpdate
|> Program.withConsoleTrace
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
