module Events

open Domain

type Event =
  | VotingWasIssued of Voting
  | VotingWasRevoked of Voting
  | VotingPeriodWasFinished
  | AbstractWasProposed of ConferenceAbstract
  | AbstractWasAccepted of AbstractId
  | AbstractWasRejected of AbstractId
