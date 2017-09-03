module Events

open Model

type Event =
  | VotingWasIssued of Voting
  | VotingPeriodWasFinished
  | AbstractWasProposed of ConferenceAbstract
  | AbstractWasAccepted of AbstractId
  | AbstractWasRejected of AbstractId
  | FinishingDenied of string
  | VotingDenied of string
  | ProposingDenied of string
