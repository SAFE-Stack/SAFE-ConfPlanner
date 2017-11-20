module Conference.Api.API

open Model

type Conferences =
  (ConferenceId * string) list

type QueryParameter =
  | Conference of ConferenceId
  | Conferences

type QueryResult =
  | Conference of Conference
  | Conferences of Conferences
  | ConferenceNotFound
