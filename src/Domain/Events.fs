module Events

open Model

type Event =
  | OrganizerRegistered of Organizer
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
