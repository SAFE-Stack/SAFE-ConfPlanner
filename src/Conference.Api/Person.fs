module Conference.Api.Person

open API
open Infrastructure.Types
open Domain.Events
open Domain.Model
open Domain.Projections

type PersonReadModel =
  Map<StreamId,Person>

let private evolveState state (streamId : StreamId ,events) : PersonReadModel =
  let person =
    state
    |> Map.tryFind streamId
    |> Option.defaultValue (emptyPerson())

  state
  |> Map.add streamId (events |> List.fold Person.apply person)

let projection : ProjectionDefinition<PersonReadModel, Event>=
  {
    InitialState = Map.empty
    UpdateState = evolveState
  }

let queryHandler (query : Query<QueryParameter>) (readModel : PersonReadModel) : QueryHandled<QueryResult> =
  match query.Parameter with
  | QueryParameter.Person personId ->
      let person =
        readModel
        |> Map.tryPick (fun _ person ->
                          if person.Id = personId then
                            person |> Some
                          else None)

      match person with
      | Some person ->
          person
          |> Person
          |> Handled

      | None ->
          PersonNotFound
          |> Handled

  | _ -> NotHandled




