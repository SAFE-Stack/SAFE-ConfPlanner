module Support.Conference1

open System
open Events
open Model

open Support.Helper

let private conference =
  emptyConference()
  |> (fun conf -> { conf with Id = ConferenceId <| System.Guid.Parse "37b5252a-8887-43bb-87a0-62fbf8d21799" })

let private heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
let private fellien = { Firstname = "Janek";  Lastname = "Felien"; Id = OrganizerId <| Guid.NewGuid() }
let private poepke = { Firstname = "Conrad";  Lastname = "Poepke"; Id = OrganizerId <| Guid.NewGuid() }

let private talk1 = proposedTalk "Event Storming ftw" "Alberto" "Brandolino"
let private talk2 = proposedTalk "F# ftw" "Don" "Syme"
let private talk3 = proposedTalk "DDD ftw" "Eric" "Evans"

let private events =
  [
    ConferenceScheduled conference
    OrganizerRegistered heimeshoff
    OrganizerRegistered fellien
    OrganizerRegistered poepke
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (vote talk3 heimeshoff Points.Two)
    VotingWasIssued (veto talk3 fellien)
    VotingWasIssued (vote talk3 poepke Points.Two)
    VotingWasIssued (vote talk2 heimeshoff Points.One)
    VotingWasIssued (vote talk2 fellien Points.One)
    VotingWasIssued (vote talk2 poepke Points.One)
    VotingWasIssued (vote talk1 heimeshoff Points.Zero)
    VotingWasIssued (vote talk1 fellien Points.Zero)
    VotingWasIssued (vote talk1 poepke Points.Zero)
  ]

let eventSets () =
  events
  |> makeEventSets (makeStreamId conference.Id)
