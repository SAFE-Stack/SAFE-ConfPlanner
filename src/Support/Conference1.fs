module Support.Conference1

open System
open Domain.Events
open Domain.Model

open Support.Helper

let private conference =
  emptyConference()
  |> (fun conf -> { conf with Id = ConferenceId <| System.Guid.Parse "37b5252a-8887-43bb-87a0-62fbf8d21799" })
  |> (fun conf -> { conf with Title = "Kandddinsky" })

let private heimeshoff = person "Marco" "Heimeshoff" "311b9fbd-98a2-401e-b9e9-bab15897dad4"
let private fellien = person "Janek" "Felien" (Guid.NewGuid() |> string)
let private poepke = person "Conrad" "Poepke" (Guid.NewGuid() |> string)

let private talk1 = proposedTalk "Event Storming ftw" "Alberto" "Brandolino"
let private talk2 = proposedTalk "F# ftw" "Don" "Syme"
let private talk3 = proposedTalk "DDD ftw" "Eric" "Evans"

let private events =
  [
    PersonRegistered heimeshoff
    PersonRegistered fellien
    PersonRegistered poepke
    ConferenceScheduled conference
    OrganizerAddedToConference heimeshoff.Id
    OrganizerAddedToConference fellien.Id
    OrganizerAddedToConference poepke.Id
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteTwo talk3 heimeshoff.Id)
    VotingWasIssued (veto talk3 fellien.Id)
    VotingWasIssued (voteTwo talk3 poepke.Id)
    VotingWasIssued (voteOne talk2 heimeshoff.Id)
    VotingWasIssued (voteOne talk2 fellien.Id)
    VotingWasIssued (voteOne talk2 poepke.Id)
    VotingWasIssued (voteZero talk1 heimeshoff.Id)
    VotingWasIssued (voteZero talk1 fellien.Id)
    VotingWasIssued (voteZero talk1 poepke.Id)
  ]

let eventSets () =
  events
  |> makeEventSets (makeStreamId conference.Id)
