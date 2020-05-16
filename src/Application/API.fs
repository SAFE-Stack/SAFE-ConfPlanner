namespace Application

open Domain.Model
open EventSourced
open Domain.Commands

module API =

  type QueryParameter =
    | Conference of ConferenceId
    | Organizers
    | Conferences

  type Conferences =
    (ConferenceId * string) list

  type QueryError =
    | ConferenceNotFound


  type CommandApi<'Command,'Event> = {
    Handle : CommandEnvelope<'Command> -> Async<Result<EventEnvelope<'Event> list,string>>
  }
  with
    static member RouteBuilder _ m = sprintf "/api/command/%s" m

  type ConferenceQueryApi = {
    conference : ConferenceId -> Async<Result<Conference, QueryError>>
    conferences : unit -> Async<Result<Conferences, QueryError>>
  }
  with
    static member RouteBuilder _ m = sprintf "/api/conference/query%s" m


  type OrganizerQueryApi = {
    organizers : unit -> Async<Result<Organizer list, QueryError>>
  }
  with
    static member RouteBuilder _ m = sprintf "/api/organizer/query%s" m


  type ConferenceCommandApi = {
    ScheduleConference : Conference -> ConferenceId -> CommandEnvelope<Command>
    ChangeTitle : string -> ConferenceId -> CommandEnvelope<Command>
    DecideNumberOfSlots : int -> ConferenceId -> CommandEnvelope<Command>
    AddOrganizerToConference : Organizer -> ConferenceId -> CommandEnvelope<Command>
    RemoveOrganizerFromConference : Organizer -> ConferenceId -> CommandEnvelope<Command>
    Vote : Voting -> ConferenceId -> CommandEnvelope<Command>
    RevokeVoting : Voting -> ConferenceId -> CommandEnvelope<Command>
    FinishVotingPeriod : ConferenceId -> CommandEnvelope<Command>
    ReopenVotingPeriod : ConferenceId -> CommandEnvelope<Command>
    ProposeAbstract : ConferenceAbstract -> ConferenceId -> CommandEnvelope<Command>
  }

  module Command =
    let private envelope (ConferenceId eventSource) command =
      {
        Transaction = TransactionId.New()
        EventSource = eventSource
        Command = command
      }

    let conferenceApi : ConferenceCommandApi =
      {
        ScheduleConference = fun conference conferenceId -> envelope conferenceId (ScheduleConference conference)
        ChangeTitle = fun title conferenceId -> envelope conferenceId (ChangeTitle title)
        DecideNumberOfSlots = fun title conferenceId -> envelope conferenceId (DecideNumberOfSlots title)
        AddOrganizerToConference = fun organizer conferenceId -> envelope conferenceId (AddOrganizerToConference organizer)
        RemoveOrganizerFromConference = fun organizer conferenceId -> envelope conferenceId (RemoveOrganizerFromConference organizer)
        Vote = fun voting conferenceId -> envelope conferenceId (Vote voting)
        RevokeVoting = fun voting conferenceId -> envelope conferenceId (RevokeVoting voting)
        FinishVotingPeriod = fun conferenceId -> envelope conferenceId FinishVotingPeriod
        ReopenVotingPeriod = fun conferenceId -> envelope conferenceId ReopenVotingPeriod
        ProposeAbstract = fun conferenceAbstract conferenceId -> envelope conferenceId (ProposeAbstract conferenceAbstract)
      }


module CQN =
  open API

  let commandPort commandHandler: CommandApi<_,_> =
    {
      Handle = fun commandEnvelope -> commandHandler commandEnvelope
    }

