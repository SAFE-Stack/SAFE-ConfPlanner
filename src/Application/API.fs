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


    type ConferenceApi = {
      conference : ConferenceId -> Async<Result<Conference, QueryError>>
      conferences : unit -> Async<Result<Conferences, QueryError>>
    }

    type OrganizerApi = {
      organizers : unit -> Async<Result<Organizer list, QueryError>>
    }
