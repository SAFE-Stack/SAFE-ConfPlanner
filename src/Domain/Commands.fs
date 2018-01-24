module Domain.Commands

open Model

type Command =
  | RegisterPerson of Person
  | ScheduleConference of Conference
  | ChangeTitle of string
  | DecideNumberOfSlots of int
  | AddOrganizerToConference of PersonId
  | RemoveOrganizerFromConference of PersonId
  | Vote of Voting
  | RevokeVoting of Voting
  | FinishVotingPeriod
  | ReopenVotingPeriod
  | ProposeAbstract of ConferenceAbstract
  | AcceptAbstract of ConferenceAbstract
  | RejectAbstract of ConferenceAbstract
