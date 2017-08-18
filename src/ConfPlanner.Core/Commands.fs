module Commands

open Domain

type Command =
  | ProposeAbstract of ProposedAbstract
  | Vote of Voting
  | RevokeVoting of Voting
  | OpenVotingPeriod
  | FinishVotingPeriod
  | AcceptAbstract of ProposedAbstract
  | RejectAbstract of ProposedAbstract