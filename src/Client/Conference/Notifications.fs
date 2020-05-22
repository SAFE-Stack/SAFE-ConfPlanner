module Conference.Notifications

open Domain
open Thoth.Elmish
open EventSourced
open System

let private notificationType notification builder =
  match notification with
  | Events.Error _ ->
      Toast.error builder

  | _ ->
      Toast.success builder


let fromEventEnvelopes eventEnvelope =
  Toast.message (Events.toString eventEnvelope.Event)
  |> Toast.position Toast.TopRight
  |> Toast.timeout (TimeSpan.FromSeconds (3.0))
  |> Toast.withCloseButton
  |> notificationType eventEnvelope.Event
