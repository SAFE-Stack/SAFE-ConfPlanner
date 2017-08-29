module SSV.State

open Elmish
open Types


let defaultModel() =
  [
      {
        Element =
          { Wochen = [SSV_default()]
            Zeitraumgruppe = Immer }
        Prioritaet = 1
      }
  ]

let init () : Model * Cmd<Msg> =
  defaultModel(), []

let update msg model : Model * Cmd<Msg> =
  match msg with
  | Tag_Stundenwert_geaendert (wochentag,wert) ->
      let value =
        match System.Double.TryParse wert with
        | (true, v) -> v
        | _ -> 0.
      let updateWoche wochentag woche =
        match wochentag with
        | Montag -> { woche with Montag = value }
        | Dienstag -> { woche with Dienstag = value }
        | Mittwoch -> { woche with Mittwoch = value }
        | Donnerstag -> { woche with Donnerstag = value }
        | Freitag -> { woche with Freitag = value }
        | Samstag -> { woche with Samstag = value }
        | Sonntag -> { woche with Sonntag = value }

      let updateElement (element : SSV_Element) =
        let woche =
          element.Wochen
          |> List.head
          |> updateWoche wochentag
        { element with Wochen = [ woche ] }

      let first = model |> List.head
      [ { first with Element = first.Element |> updateElement} ], Cmd.none

  | Woche_hinzufuegen ->
      let updateElement (element : SSV_Element) =
        { element with Wochen = element.Wochen @ [SSV_default()] }

      let first = model |> List.head
      [{ first with Element = first.Element |> updateElement}], Cmd.none