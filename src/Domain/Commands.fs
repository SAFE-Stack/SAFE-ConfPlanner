module Domain.Commands

open Model

type Command =
  | ScheduleConference of Conference
  | ChangeTitle of string
  | DecideNumberOfSlots of int
  | AddOrganizerToConference of Organizer
  | RemoveOrganizerFromConference of Organizer
  | Vote of Voting
  | RevokeVoting of Voting
  | FinishVotingPeriod
  | ReopenVotingPeriod
  | ProposeAbstract of ConferenceAbstract
