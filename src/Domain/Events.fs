module Events

open Model

type Event =
  | ConferenceScheduled of Conference
  | ConferenceAlreadyScheduled
  | OrganizerAddedToConference of Organizer
  | OrganizerAlreadyAddedToConference of Organizer
  | OrganizerRemovedFromConference of Organizer
  | OrganizerWasNotAddedToConference of Organizer
  | TalkWasProposed of ConferenceAbstract
  | CallForPapersOpened
  | CallForPapersClosed
  | TitleChanged of string
  | NumberOfSlotsDecided of int
  | VotingWasIssued of Voting
  | VotingWasRevoked of Voting
  | RevocationOfVotingWasDenied of Voting * error : string
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
  | OrganizerAlreadyAddedToConference _ -> true
  | OrganizerRemovedFromConference _ -> true
  | OrganizerWasNotAddedToConference _ -> false
  | TalkWasProposed _ -> false
  | CallForPapersOpened -> false
  | CallForPapersClosed -> false
  | TitleChanged _ -> false
  | NumberOfSlotsDecided _ -> false
  | VotingWasIssued _ -> false
  | VotingWasRevoked _ -> false
  | RevocationOfVotingWasDenied _ -> true
  | VotingDenied _ -> true
  | VotingPeriodWasFinished -> false
  | VotingPeriodWasReopened -> false
  | AbstractWasProposed _ -> false
  | AbstractWasAccepted _ -> false
  | AbstractWasRejected _ -> false
  | FinishingDenied _ -> true
  | ProposingDenied _ -> true
