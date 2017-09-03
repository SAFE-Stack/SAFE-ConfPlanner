module Commands

open Domain

type Command =
  | Vote of Voting
  | RevokeVoting of Voting
  | FinishVotingPeriod
  | ProposeAbstract of ConferenceAbstract
  | AcceptAbstract of ConferenceAbstract
  | RejectAbstract of ConferenceAbstract