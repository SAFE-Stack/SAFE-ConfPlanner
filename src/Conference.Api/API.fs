module Conference.Api.API

open Model

type Conferences =
  (ConferenceId * string) list

type QueryParameter =
  | Conference of ConferenceId
  | Organizers
  | Conferences

type QueryResult =
  | Conference of Conference
  | Conferences of Conferences
  | Organizers of Organizers
  | ConferenceNotFound
