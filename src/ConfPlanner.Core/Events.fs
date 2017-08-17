module Events

open Domain

type Event =
  | VotingWasIssued of Voting
  | VotingWasRevoked of Voting
  | VotingPeriodWasFinished
  | AbstractWasProposed of ProposedAbstract
  | AbstractWasAccepted of ProposedAbstract
  | AbstractWasRejected of ProposedAbstract
