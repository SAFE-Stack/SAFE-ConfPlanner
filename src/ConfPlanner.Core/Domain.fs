module Domain

open System

type SpeakerId = Speaker of Guid
type Speaker = {
  Id : SpeakerId
  Firstname : string
  Lastname : string
}

type AbstractId = AbstractId of Guid

type AbstractData = {
  Id : AbstractId
  Duration : float
  Speakers : Speaker list
  Text : String
}

type Abstract =
  | Proposed of ProposedAbstract
  | Accepted of AcceptedAbstract

and ProposedAbstract =
  | Talk of AbstractData
  | HandsOnSession of AbstractData

and AcceptedAbstract =
  | Talk of AbstractData
  | HandsOnSession of AbstractData

type OrganizerId = OrganizerId of Guid

type Organizer = {
  Id : OrganizerId
  Firstname : string
  Lastname : string
}

type Voting =
  | Vote of ProposedAbstract*OrganizerId
  | Veto of ProposedAbstract*OrganizerId

type VotingResults = Voting list

type Abstracts = Abstract list

// type AcceptAbstract = ProposedAbstract -> AcceptedAbstract

// type Voter = Voting -> VotingResults -> VotingResults

type CallForPapers =
  | NotOpened
  | Open
  | Closed

type VotingPeriod =
  | InProgess
  | Finished

type ConferenceId = ConferenceId of Guid

type Conference = {
  Id : ConferenceId
  CallForPapers : CallForPapers
  VotingPeriod : VotingPeriod
  Abstracts : Abstracts
  VotingResults : VotingResults
  Organizers : Organizer list
}
