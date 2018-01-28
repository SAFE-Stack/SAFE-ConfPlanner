module Support.Conference2

open System
open Domain.Events
open Domain.Model

open Support.Helper

let private conference =
  emptyConference()
  |> (fun conf -> { conf with Title = "Be Sharps" })

let private heimeshoff = person "Marco" "Heimeshoff" "311b9fbd-98a2-401e-b9e9-bab15897dad4"
let private sachse = person "Roman" "Sachse" (Guid.NewGuid() |> string)
let private helmig = person "Nils" "Helmig" (Guid.NewGuid() |> string)

let private talk1 = proposedTalk "Elmish with React Native" "Steffen" "Forkmann"
let private talk2 = proposedTalk "Everything Fable" "Maxime" "Mangel"
let private talk3 = proposedTalk "Azure and everything" "Isaac" "Abraham"

let private events =
  [
    PersonRegistered heimeshoff
    PersonRegistered sachse
    PersonRegistered helmig
    ConferenceScheduled conference
    OrganizerAddedToConference heimeshoff.Id
    OrganizerAddedToConference sachse.Id
    OrganizerAddedToConference helmig.Id
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteTwo talk3 heimeshoff.Id)
    VotingWasIssued (veto talk3 sachse.Id)
    VotingWasIssued (voteTwo talk3 helmig.Id)
    VotingWasIssued (voteOne talk2 heimeshoff.Id)
    VotingWasIssued (voteOne talk2 sachse.Id)
    VotingWasIssued (voteOne talk2 helmig.Id)
    VotingWasIssued (voteZero talk1 heimeshoff.Id)
    VotingWasIssued (voteZero talk1 sachse.Id)
    VotingWasIssued (voteZero talk1 helmig.Id)
  ]

let eventSets () =
  events
  |> makeEventSets (makeStreamId conference.Id)
