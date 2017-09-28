module Conference.View

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

type RemoteData<'Result> =
  | NotAsked
  | Loading
  | Failure of string
  | Success of 'Result

type Model =
  {
    Info : string
    State : RemoteData<Model.Conference>
    Events : Events.Event list
    LastQueryResponse : QueryResponse<Dummy.QueryResult> option
  }

type Msg =
  | Received of ServerMsg<Events.Event,Dummy.QueryResult>
  | QueryState
  | QueryStateTimesX of int
  | QueryCanNotBeHandled

let mutable private sendPerWebsocket : ClientMsg<Dummy.Command,Dummy.QueryParameter,Dummy.QueryResult> -> unit =
  fun _ -> failwith "WebSocket not connected"

let private startWs dispatch =
  let onMsg : System.Func<MessageEvent, obj> =
    (fun (wsMsg:MessageEvent) ->
      let msg =  ofJson<ServerMsg<Events.Event,Dummy.QueryResult>> <| unbox wsMsg.data
      Msg.Received msg |> dispatch
      null) |> unbox // temporary fix until Fable WS Import is upgraded to Fable 1.*

  let ws = Fable.Import.Browser.WebSocket.Create("ws://127.0.0.1:8085/websocket")

  let send msg =
    ws.send (toJson msg)

  ws.onopen <- (fun _ -> send (ClientMsg.Connect) ; null)
  ws.onmessage <- onMsg
  ws.onerror <- (fun err -> printfn "%A" err ; null)

  sendPerWebsocket <- send

  ()

let proposedTalk(title): Model.ConferenceAbstract =
   {
      Id = Model.AbstractId <| Guid.NewGuid()
      Duration = 45.
      Speakers = []
      Text = title
      Status = Model.AbstractStatus.Proposed
      Type = Model.AbstractType.Talk
   }

let init() =
  {
    Info = "noch nicht connected"
    State = NotAsked
    Events = [
      Events.AbstractWasProposed (proposedTalk "DDD is nize")
      Events.AbstractWasProposed (proposedTalk "EventStorm im Wasserglas")
      Events.AbstractWasProposed (proposedTalk "Roman... ist... tot")
      Events.AbstractWasProposed (proposedTalk "Twinziiies")
      ]
    LastQueryResponse = None
  }, Cmd.ofSub startWs


let wsCmd cmd =
  [fun _ -> sendPerWebsocket cmd]

let transactionId() =
  TransactionId <| System.Guid.NewGuid()

let createQuery query =
  {
    Query.Id = QueryId <| System.Guid.NewGuid()
    Query.Parameter = query
  }


let updateStateWithEvents state events =
  match state with
  | Success state ->
       events
        |> List.fold Projections.apply state
        |> Success

  | _ -> state


let talk (t:Model.ConferenceAbstract): React.ReactElement =
  div [] [ t.Text |> str ]


let root model dispatch =
  div []
    [
      div [ ClassName "columns is-vcentered" ]
        [ div [ ClassName "column"]
            [ "Abstracts" |> str ]
          div [ ClassName "column"]
            [ "Accepted" |> str ]
          div [ ClassName "column"]
            [ "Rejected" |> str ] ]
      div [ ClassName "columns is-vcentered" ]
        [ div [ ClassName "column";
            Style [
              BackgroundColor "#dddddd"
              Display Flex
              FlexDirection "column"
              ]]
            (match model.State with
             | Success s -> s.Abstracts |> List.map talk
             | _ -> [] )
          div [ ClassName "column";
              Style [
              BackgroundColor "#ddffdd"
              Display Flex
              FlexDirection "column"]]
            [ ]
          div [ ClassName "column";
                        Style [
              BackgroundColor "#ffdddd"
              Display Flex
              FlexDirection "column"]]
            [ ] ] ]
