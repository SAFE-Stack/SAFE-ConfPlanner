module Conference.View

open Fable.Helpers.React
open Conference.Types

let root model dispatch =
  div
    []
    [ model.Something |> str]
