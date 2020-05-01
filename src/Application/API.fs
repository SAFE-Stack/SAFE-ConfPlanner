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
  type QueryResult =
    | Conference of Conference
    | Conferences of Conferences
    | Organizers of Organizers
    | ConferenceNotFound
