module Conference.Api.Conference

open API
open Infrastructure.Types
open Events
open Model
open Projections

type ConferenceReadModel =
  Map<StreamId,Conference>

let private evolveState state (streamId : StreamId ,events) : ConferenceReadModel =
  let conference =
    state
    |> Map.tryFind streamId
    |> Option.defaultValue (emptyConference())

  state
  |> Map.add streamId (events |> List.fold apply conference)

let projection : ProjectionDefinition<ConferenceReadModel, Event>=
  {
    InitialState = Map.empty
    UpdateState = evolveState
  }

let queryHandler (query : Query<QueryParameter>) (readModel : ConferenceReadModel) : QueryHandled<QueryResult> =
  match query.Parameter with
  | QueryParameter.Conference conferenceId ->
      let conference =
        readModel
        |> Map.tryPick (fun _ conference ->
                          if conference.Id = conferenceId then
                            conference |> Some
                          else None)

      match conference with
      | Some conference ->
          conference
          |> Conference
          |> Handled

      | None ->
          ConferenceNotFound
          |> Handled

  | _ -> NotHandled




