[<RequireQualifiedAccess>]
module Conference.Api

open Domain.Commands
open Domain.Events
open Conference.Types
open EventSourced
open Fable.Remoting.Client
open Application
open Elmish

let private conferenceApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.ConferenceQueryApi.RouteBuilder
  |> Remoting.buildProxy<Application.API.ConferenceQueryApi>

let private organizerApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.OrganizerQueryApi.RouteBuilder
  |> Remoting.buildProxy<Application.API.OrganizerQueryApi>

let private commandPort =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.CommandApi<_,_>.RouteBuilder
  |> Remoting.buildProxy<Application.API.CommandApi<Command,Event>>

let sendCommand commandEnvelope : Async<Msg> =
  async {
    match! commandPort.Handle commandEnvelope with
    | Ok eventEnvelopes ->
        return CommandResponse (commandEnvelope.Transaction, Ok eventEnvelopes)

    | Result.Error error ->
        return CommandResponse (commandEnvelope.Transaction, Result.Error error)
  }


let commandEnvelopeForMsg conferenceId msg =
  match msg with
  | Vote voting ->
      API.Command.conferenceApi.Vote voting conferenceId

  | RevokeVoting voting ->
      API.Command.conferenceApi.RevokeVoting voting conferenceId

  | FinishVotingperiod ->
      API.Command.conferenceApi.FinishVotingPeriod conferenceId

  | ReopenVotingperiod ->
      API.Command.conferenceApi.ReopenVotingPeriod conferenceId

  | AddOrganizerToConference organizer ->
      API.Command.conferenceApi.AddOrganizerToConference organizer conferenceId

  | RemoveOrganizerFromConference organizer ->
      API.Command.conferenceApi.RemoveOrganizerFromConference organizer conferenceId

  | ChangeTitle title ->
      API.Command.conferenceApi.ChangeTitle title conferenceId

  | DecideNumberOfSlots number ->
     API.Command.conferenceApi.DecideNumberOfSlots number conferenceId


let queryConference conferenceId =
  // TODO react to query Error
  Cmd.OfAsync.perform conferenceApi.conference conferenceId ConferenceLoaded

let queryConferences =
  Cmd.OfAsync.perform conferenceApi.conferences () ConferencesLoaded

let queryOrganizers =
  Cmd.OfAsync.perform organizerApi.organizers () OrganizersLoaded


module Local =
  let behaviourFor msg =
    match msg with
    | Vote voting ->
        Domain.Behaviour.vote voting

    | RevokeVoting voting ->
        Domain.Behaviour.revokeVoting voting

    | FinishVotingperiod ->
        Domain.Behaviour.finishVotingPeriod

    | ReopenVotingperiod ->
        Domain.Behaviour.reopenVotingPeriod

    | AddOrganizerToConference organizer ->
        Domain.Behaviour.addOrganizerToConference organizer

    | RemoveOrganizerFromConference organizer ->
        Domain.Behaviour.removeOrganizerFromConference organizer

    | ChangeTitle title ->
        Domain.Behaviour.changeTitle title

    | DecideNumberOfSlots number ->
        Domain.Behaviour.decideNumberOfSlots number


  let evolve conference events  =
    events |> List.fold Domain.Projections.evolve conference
