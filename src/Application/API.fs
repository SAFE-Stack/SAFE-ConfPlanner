namespace Application

open Domain.Model
open EventSourced

module API =
  type QueryParameter =
    | Conference of ConferenceId
    | Organizers
    | Conferences

  type Conferences =
    (ConferenceId * string) list

  type QueryError =
    | ConferenceNotFound


  type ConferenceQueryApi = {
    conference : ConferenceId -> Async<Result<Conference, QueryError>>
    conferences : unit -> Async<Result<Conferences, QueryError>>
  }


  type OrganizerApi = {
    organizers : unit -> Async<Result<Organizer list, QueryError>>
  }

  type ConferenceCommandApi = {
    // TODO return transactionid
    ScheduleConference : Conference -> ConferenceId -> Async<Result<unit,string>>
    ChangeTitle : string -> ConferenceId -> Async<Result<unit,string>>
    DecideNumberOfSlots : int -> ConferenceId -> Async<Result<unit,string>>
    AddOrganizerToConference : Organizer -> ConferenceId -> Async<Result<unit,string>>
    RemoveOrganizerFromConference : Organizer -> ConferenceId -> Async<Result<unit,string>>
    Vote : Voting -> ConferenceId -> Async<Result<unit,string>>
    RevokeVoting : Voting -> ConferenceId -> Async<Result<unit,string>>
    FinishVotingPeriod : ConferenceId -> Async<Result<unit,string>>
    ReopenVotingPeriod : ConferenceId -> Async<Result<unit,string>>
    ProposeAbstract : ConferenceAbstract -> ConferenceId -> Async<Result<unit,string>>
  }
  with
    static member RouteBuilder _ m = sprintf "/api/conference/command/%s" m


  let conferenceRouteBuilder _ m = sprintf "/api/conference/query%s" m
  let organizerRouteBuilder _ m = sprintf "/api/organizer/query%s" m
