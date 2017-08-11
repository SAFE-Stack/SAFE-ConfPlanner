module Commands

open Domain

type Command =
  | Vote of Voting
  | RevokeVoting of Voting
  | FinishVotingPeriod
  | AcceptAbstract of ProposedAbstract
  | RejectAbstract of ProposedAbstract