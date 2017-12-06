module Info.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma.Layouts

let view =
  Section.section []
    [
      Container.container [ Container.isFluid ]
        [
          div
            [ ClassName "content" ]
            [
              h1 [] [ str "About page" ]
              p [] [ str "This template is a simple application build with Fable + Elmish + React." ]
            ]
        ]
    ]

