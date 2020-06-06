module Support.Conference2

open System
open Domain.Events
open Domain.Model
open Support.Helper
open Application.Organizers


let conferenceId = ConferenceId <| System.Guid.Parse "23A7C9C6-C25E-4955-A60A-58C8074EDEA2"

let private conference =
  emptyConference()
  |> (fun conf -> { conf with Id = conferenceId })
  |> (fun conf -> { conf with Title = "Be Sharps" })

let private talk1 = proposedTalk "Elmish with React Native" "Steffen" "Forkmann"
let private talk2 = proposedTalk "Everything Fable" "Maxime" "Mangel"
let private talk3 = proposedTalk "SAFE Stack" "Isaac" "Abraham"

let events =
  [
    ConferenceScheduled conference
    OrganizerAddedToConference roman
    OrganizerAddedToConference marco
    OrganizerAddedToConference dylan
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteTwo talk3 roman)
    VotingWasIssued (veto talk3 roman)
    VotingWasIssued (voteTwo talk3 dylan)
    VotingWasIssued (voteOne talk2 roman)
    VotingWasIssued (voteOne talk2 roman)
    VotingWasIssued (voteOne talk2 dylan)
    VotingWasIssued (voteZero talk1 roman)
    VotingWasIssued (voteZero talk1 roman)
    VotingWasIssued (voteZero talk1 dylan)
  ]
