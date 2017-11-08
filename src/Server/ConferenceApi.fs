module ConferenceApi

open Infrastructure.Types
open Events
open Model
open Projections

type QueryParameter =
  | State
  | CanNotBeHandled

type QueryResult =
  | State of Conference

let projection : Projection<Conference, Event>=
  {
    InitialState = emptyConference()
    UpdateState = apply
  }

let queryHandler (query : Query<QueryParameter>) (state : Conference) : QueryHandled<QueryResult> =
  match query.Parameter with
  | QueryParameter.State ->
      state
      |> QueryResult.State
      |> Handled

  | _ -> NotHandled




