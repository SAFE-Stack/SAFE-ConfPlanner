module Events

open Model

type Event =
  | ConferenceScheduled of Conference
  | ConferenceAlreadyScheduled
  | OrganizerAddedToConference of Organizer
  | OrganizerAlreadyAddedToConference
  | TalkWasProposed of ConferenceAbstract
  | CallForPapersOpened
  | CallForPapersClosed
  | NumberOfSlotsDecided of int
  | VotingWasIssued of Voting
  | VotingPeriodWasFinished
  | VotingPeriodWasReopened
  | AbstractWasProposed of ConferenceAbstract
  | AbstractWasAccepted of AbstractId
  | AbstractWasRejected of AbstractId
  | FinishingDenied of string
  | VotingDenied of string
  | ProposingDenied of string

let isError event =
  match event with
  | ConferenceScheduled _ -> false
  | ConferenceAlreadyScheduled -> true
  | OrganizerAddedToConference _ -> false
  | OrganizerAlreadyAddedToConference -> true
  | TalkWasProposed _ -> false
  | CallForPapersOpened -> false
  | CallForPapersClosed -> false
  | NumberOfSlotsDecided _ -> false
  | VotingWasIssued _ -> false
  | VotingPeriodWasFinished -> false
  | VotingPeriodWasReopened -> false
  | AbstractWasProposed _ -> false
  | AbstractWasAccepted _ -> false
  | AbstractWasRejected _ -> false
  | FinishingDenied _ -> true
  | VotingDenied _ -> true
  | ProposingDenied _ -> true
