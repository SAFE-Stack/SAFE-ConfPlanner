module Conference.Api.Conferences

open API
open Infrastructure.Types
open Events
open Model

type ConferencesReadModel =
  (ConferenceId * string) list

let private apply readModel event =
  match event with
    | ConferenceScheduled conference ->
        (conference.Id, conference.Title) :: readModel

    | _ ->
        readModel

let private evolve readModel (streamId : StreamId ,events) =
  events |> List.fold apply readModel

let projection : ProjectionDefinition<ConferencesReadModel, Event>=
  {
    InitialState = []
    UpdateState = evolve
  }

let queryHandler (query : Query<QueryParameter>) (readModel : ConferencesReadModel) : QueryHandled<QueryResult> =
  match query.Parameter with
  | QueryParameter.Conferences ->
      readModel
      |> Conferences
      |> Handled

  | _ -> NotHandled




