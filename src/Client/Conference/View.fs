module Conference.View

open Conference.Types

open System
open Elmish
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser
open Fable.Helpers.React
open Fable.Helpers.React.Props

open Infrastructure.Types
open Server.ServerTypes
open Global

let proposedTalk(title): Model.ConferenceAbstract =
   {
      Id = Model.AbstractId <| Guid.NewGuid()
      Duration = 45.
      Speakers = []
      Text = title
      Status = Model.AbstractStatus.Proposed
      Type = Model.AbstractType.Talk
   }


let talk (t:Model.ConferenceAbstract) =
  div [] [ t.Text |> str ]


let root (model: Model) dispatch =
  div []
    [
      div [ ClassName "columns is-vcentered" ]
        [
          div [ ClassName "column"] [ "Abstracts" |> str ]
          div [ ClassName "column"] [ "Accepted" |> str ]
          div [ ClassName "column"] [ "Rejected" |> str ]
        ]
      div [ ClassName "columns is-vcentered" ]
        [ div
            [
              ClassName "column"
              Style
                [
                  BackgroundColor "#dddddd"
                  Display Flex
                  FlexDirection "column"
                ]
            ]
            (match model.State with
             | Success s -> s.Abstracts |> List.map talk
             | _ -> [] )
          div
            [
              ClassName "column"
              Style
                [
                  BackgroundColor "#ddffdd"
                  Display Flex
                  FlexDirection "column"
                ]
            ]
            []
          div
            [
              ClassName "column"
              Style
                [
                  BackgroundColor "#ffdddd"
                  Display Flex
                  FlexDirection "column"
                ]
            ]
            []
        ]
    ]
