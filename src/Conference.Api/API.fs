module Conference.Api.API

open Model

type QueryParameter =
  | Conference of ConferenceId
  | Conferences
  | CanNotBeHandled

type QueryResult =
  | Conference of Conference
  | Conferences of (ConferenceId * string) list
  | ConferenceNotFound
