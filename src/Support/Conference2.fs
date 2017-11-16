module Support.Conference2

open System
open Events
open Model

open Support.Helper

let private conference =
  emptyConference()
  |> (fun conf -> { conf with Title = "Be Sharps" })

let private heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
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

    VotingWasIssued (vote talk3 heimeshoff Points.Two)
    VotingWasIssued (veto talk3 sachse)
    VotingWasIssued (vote talk3 helmig Points.Two)
    VotingWasIssued (vote talk2 heimeshoff Points.One)
    VotingWasIssued (vote talk2 sachse Points.One)
    VotingWasIssued (vote talk2 helmig Points.One)
    VotingWasIssued (vote talk1 heimeshoff Points.Zero)
    VotingWasIssued (vote talk1 sachse Points.Zero)
    VotingWasIssued (vote talk1 helmig Points.Zero)
  ]

let eventSets () =
  events
  |> makeEventSets (makeStreamId conference.Id)
