module Support.Conference1

open Domain.Events
open Domain.Model
open Support.Helper
open Application.Organizers

let conferenceId = ConferenceId <| System.Guid.Parse "37b5252a-8887-43bb-87a0-62fbf8d21799"

let private conference =
  emptyConference()
  |> (fun conf -> { conf with Id = conferenceId })
  |> (fun conf -> { conf with Title = "F# Conf" })


let private talk1 = proposedTalk "Hedy: A gradual programming language for education" "Felienne" "Hermans"
let private talk2 = proposedTalk "On Independence!" "Don" "Syme"
let private talk3 = proposedTalk "From Zero to F# Hero" "James" "Randal"

let events =
  [
    ConferenceScheduled conference
    OrganizerAddedToConference roman
    OrganizerAddedToConference marco
    OrganizerAddedToConference gien
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteTwo talk3 roman)
    VotingWasIssued (veto talk3 marco)
    VotingWasIssued (voteTwo talk3 gien)
    VotingWasIssued (voteOne talk2 roman)
    VotingWasIssued (voteOne talk2 marco)
    VotingWasIssued (voteOne talk2 gien)
    VotingWasIssued (voteZero talk1 roman)
    VotingWasIssued (voteZero talk1 marco)
    VotingWasIssued (voteZero talk1 gien)
  ]
