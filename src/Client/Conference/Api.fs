[<RequireQualifiedAccess>]
module Conference.Api

open Domain.Commands
open Domain.Events
open Conference.Types
open EventSourced
open Fable.Remoting.Client
open Application
open Elmish
open Utils.Elmish
open Utils

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
    do! Async.Sleep 500
    match! commandPort.Handle commandEnvelope with
    | Ok eventEnvelopes ->
        return CommandResponse ([commandEnvelope.Transaction], Ok eventEnvelopes)

    | Error error ->
        return CommandResponse ([commandEnvelope.Transaction], Error error)
  }

let sendCommands commandEnvelopes : Async<Msg> =
  async {
    do! Async.Sleep 500
    let transactions =
      commandEnvelopes |> List.map (fun ce -> ce.Transaction)

    match! commandPort.HandleBatch commandEnvelopes with
    | Ok eventEnvelopes ->
        return CommandResponse (transactions, Ok eventEnvelopes)

    | Error error ->
        return CommandResponse (transactions, Error error)
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
  async {
    let! result = conferenceApi.conference conferenceId
    return ConferenceQuery (Finished result)
  } |> Cmd.fromAsync

let queryConferences =
  async {
    let! result = conferenceApi.conferences ()
    return ConferencesQuery (Finished result)
  } |> Cmd.fromAsync

let queryOrganizers =
  async {
    let! result = organizerApi.organizers ()
    return OrganizersLoaded (Finished result)
  } |> Cmd.fromAsync

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
