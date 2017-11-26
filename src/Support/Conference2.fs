module Support.Conference2

open System
open Events
open Model

open Support.Helper

let private conference =
  emptyConference()
  |> (fun conf -> { conf with Title = "Be Sharps" })

let private heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.Parse "311b9fbd-98a2-401e-b9e9-bab15897dad4" }
let private sachse = { Firstname = "Roman";  Lastname = "Sachse"; Id = OrganizerId <| Guid.NewGuid() }
let private helmig = { Firstname = "Nils";  Lastname = "Helmig"; Id = OrganizerId <| Guid.NewGuid() }

let private talk1 = proposedTalk "Elmish with React Native" "Steffen" "Forkmann"
let private talk2 = proposedTalk "Everything Fable" "Maxime" "Mangel"
let private talk3 = proposedTalk "Azure and everything" "Isaac" "Abraham"

let private events =
  [
    ConferenceScheduled conference
    OrganizerRegistered heimeshoff
    OrganizerRegistered sachse
    OrganizerRegistered helmig
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteTwo talk3 heimeshoff)
    VotingWasIssued (veto talk3 sachse)
    VotingWasIssued (voteTwo talk3 helmig)
    VotingWasIssued (voteOne talk2 heimeshoff)
    VotingWasIssued (voteOne talk2 sachse)
    VotingWasIssued (voteOne talk2 helmig)
    VotingWasIssued (voteZero talk1 heimeshoff)
    VotingWasIssued (voteZero talk1 sachse)
    VotingWasIssued (voteZero talk1 helmig)
  ]

let eventSets () =
  events
  |> makeEventSets (makeStreamId conference.Id)
