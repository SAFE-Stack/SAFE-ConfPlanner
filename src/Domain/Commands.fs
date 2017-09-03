module Commands

open Model

type Command =
  | Vote of Voting
  | RevokeVoting of Voting
  | FinishVotingPeriod
  | ProposeAbstract of ConferenceAbstract
  | AcceptAbstract of ConferenceAbstract
  | RejectAbstract of ConferenceAbstract
