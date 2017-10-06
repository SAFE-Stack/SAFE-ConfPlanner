module Commands

open Model

type Command =
  | Vote of Voting
  | RevokeVoting of Voting
  | FinishVotingPeriod
  | ReopenVotingPeriod
  | ProposeAbstract of ConferenceAbstract
  | AcceptAbstract of ConferenceAbstract
  | RejectAbstract of ConferenceAbstract
