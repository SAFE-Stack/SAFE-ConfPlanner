module Conference.Notifications

open Domain.Events
open Thoth.Elmish
open EventSourced
open System

let private notificationType notification builder =
  match notification with
  | DomainError _ ->
      Toast.error builder

  | _ ->
      Toast.success builder


let fromEventEnvelopes eventEnvelope =
  Toast.message (Event.ToString eventEnvelope.Event)
  |> Toast.position Toast.TopRight
  |> Toast.timeout (TimeSpan.FromSeconds (3.0))
  |> Toast.withCloseButton
  |> notificationType eventEnvelope.Event
