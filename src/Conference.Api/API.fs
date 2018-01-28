module Conference.Api.API

open Domain.Model

type QueryParameter =
  | Conference of ConferenceId
  | Person of PersonId
  | Persons
  | Conferences

type QueryResult =
  | Conference of Conference
  | Person of Person
  | Conferences of (ConferenceId * string) list
  | Persons of Person list
  | ConferenceNotFound
  | PersonNotFound
