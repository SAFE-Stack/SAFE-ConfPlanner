module Support.Conference1

open System
open Domain.Events
open Domain.Model

open Support.Helper

let private conference =
  emptyConference()
  |> (fun conf -> { conf with Id = ConferenceId <| System.Guid.Parse "37b5252a-8887-43bb-87a0-62fbf8d21799" })
  |> (fun conf -> { conf with Title = "Kandddinsky" })

let private heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.Parse "311b9fbd-98a2-401e-b9e9-bab15897dad4" }
let private fellien = { Firstname = "Janek";  Lastname = "Felien"; Id = OrganizerId <| Guid.NewGuid() }
let private poepke = { Firstname = "Conrad";  Lastname = "Poepke"; Id = OrganizerId <| Guid.NewGuid() }

let private talk1 = proposedTalk "Event Storming ftw" "Alberto" "Brandolino"
let private talk2 = proposedTalk "F# ftw" "Don" "Syme"
let private talk3 = proposedTalk "DDD ftw" "Eric" "Evans"

let private events =
  [
    ConferenceScheduled conference
    OrganizerAddedToConference heimeshoff
    OrganizerAddedToConference fellien
    OrganizerAddedToConference poepke
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteTwo talk3 heimeshoff)
    VotingWasIssued (veto talk3 fellien)
    VotingWasIssued (voteTwo talk3 poepke)
    VotingWasIssued (voteOne talk2 heimeshoff)
    VotingWasIssued (voteOne talk2 fellien)
    VotingWasIssued (voteOne talk2 poepke)
    VotingWasIssued (voteZero talk1 heimeshoff)
    VotingWasIssued (voteZero talk1 fellien)
    VotingWasIssued (voteZero talk1 poepke)
  ]

let eventSets () =
  events
  |> makeEventSets (makeStreamId conference.Id)
