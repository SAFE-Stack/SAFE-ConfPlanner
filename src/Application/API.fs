namespace Application

open Domain.Model
open EventSourced

module API =
  type QueryParameter =
    | Conference of ConferenceId
    | Organizers
    | Conferences

  type QueryResult =
    | Conference of Conference
    | Conferences of (ConferenceId * string) list
    | Organizers of Organizers
    | ConferenceNotFound
