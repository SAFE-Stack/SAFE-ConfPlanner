module Conference.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Conference.Types

let simpleButton txt action dispatch =
  div
    [ ClassName "column" ]
    [ a
        [ ClassName "button"
          OnClick (fun _ -> action |> dispatch) ]
        [ str txt ] ]

let root model dispatch =
  div
    [ ClassName "columns is-vcentered" ]
    [ div [ ClassName "column" ] [ ]
      simpleButton "FinishVotingPerod" FinishVotingPeriod dispatch
      div [ ClassName "column" ] [ ]
    ]
