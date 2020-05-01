namespace Application

open API
open EventSourced
open Domain.Events
open Domain.Model

module Conference =
  let private relevantEvents eventEnvelope =
    match eventEnvelope.Event with
    | ConferenceScheduled _
    | OrganizerAddedToConference _
    | OrganizerRemovedFromConference _
    | TalkWasProposed _
    | CallForPapersOpened
    | CallForPapersClosed
    | TitleChanged _
    | NumberOfSlotsDecided _
    | VotingWasIssued _
    | VotingWasRevoked _
    | VotingPeriodWasFinished
    | VotingPeriodWasReopened
    | AbstractWasProposed _
    | AbstractWasAccepted _
    | AbstractWasRejected _ -> true
    | _ -> false

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
      |> List.filter relevantEvents
      |> List.fold (projectIntoMap Domain.Projections.conference) state

    InMemoryReadmodel.readModel updateState Map.empty

  let queryHandler conferences : QueryHandler<_> =
    let handleQuery query =
      match query with
      | QueryParameter.Conference (ConferenceId conferenceId) ->
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
              |> List.map (fun (_,conference) -> (conference.Id, conference.Title))
              |> Conferences
              |> box
              |> Handled
          }

      | _ ->
        async { return NotHandled }

    { Handle = handleQuery }




