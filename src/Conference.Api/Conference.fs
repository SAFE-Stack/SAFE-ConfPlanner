module Conference.Api.Conference

open API
open EventSourced


let private projectIntoMap projection =
  fun state eventEnvelope ->
    state
    |> Map.tryFind eventEnvelope.Metadata.Source
    |> Option.defaultValue projection.Init
    |> fun projectionState -> eventEnvelope.Event |> projection.Update projectionState
    |> fun newState -> state |> Map.add eventEnvelope.Metadata.Source newState


let readmodel () : InMemoryReadModel<_,_> =
  let updateState state eventEnvelopes =
    eventEnvelopes
    |> List.fold (projectIntoMap Domain.Projections.conference) state

  InMemoryReadmodel.readModel updateState Map.empty

let queryHandler conferences (query : QueryParameter) : Async<QueryResult> =
  match query with
  | QueryParameter.Conference conferenceId ->
      async {
        let! state = conferences()

        return
           match state |> Map.tryFind conferenceId with
            | Some conference ->
                conference
                |> Conference
                |> box
                |> Handled

            | None ->
                ConferenceNotFound
                |> box
                |> Handled

      }

  | QueryParameter.Conferences ->
      async {
        let! state = conferences()

        return
          state
          |> Map.toList
          |> List.map (fun (id,conference) -> (id, conference.Title))
          |> Conferences
          |> box
          |> Handled
      }

  | _ ->
    async { return NotHandled }




