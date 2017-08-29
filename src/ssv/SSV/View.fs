module SSV.View

open Fable.Core
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Types


let tagInput dispatch (wochentag : Wochentag) name wert  =
  let message_ctor msg x y =
    msg (x,y)

  input
    [ ClassName "input"
      Type "text"
      Value !^(wert |> string)
      Placeholder name
      OnChange (fun ev -> !!ev.target?value |> (message_ctor Tag_Stundenwert_geaendert wochentag) |> dispatch )
    ]

let wochenplusbutton dispatch =
  button
    [ OnClick (fun _ -> Woche_hinzufuegen |> dispatch) ] [str "+"]

let woche dispatch (model : SSV_Woche)  =
  let inputDisp = tagInput dispatch

  table
    []
    [ tbody []
        [
          tr
            []
            [
               td [] [ inputDisp Montag "Mo" model.Montag]
               td [] [ inputDisp Dienstag "Di" model.Dienstag]
               td [] [ inputDisp Mittwoch "Mi" model.Mittwoch]
               td [] [ inputDisp Donnerstag "Do" model.Donnerstag]
               td [] [ inputDisp Freitag "Fr" model.Freitag]
               td [] [ inputDisp Samstag "Sa" model.Samstag]
               td [] [ inputDisp Sonntag "So" model.Sonntag]
               td [] [ str <| ( wochenstunden model |> string )]
            ]
        ]
    ]


let element_Zuordnung dispatch (zuordnung : SSV_Element_Zuordnung) =
  div
    []
    ((zuordnung.Element.Wochen |> List.map (woche dispatch)) @
    [div [] [ wochenplusbutton dispatch ]]
    )



let view (model : SSV) dispatch =
  div
    []
    (model |> List.map (element_Zuordnung dispatch))