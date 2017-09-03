module Model

open System

type SpeakerId = Speaker of Guid
type Speaker = {
  Id : SpeakerId
  Firstname : string
  Lastname : string
}

type AbstractId = AbstractId of Guid

type AbstractStatus =
  | Proposed
  | Accepted
  | Rejected

type AbstractType =
  | Talk
  | HandsOn

type ConferenceAbstract = {
  Id : AbstractId
  Duration : float
  Speakers : Speaker list
  Text : String
  Status : AbstractStatus
  Type : AbstractType
}

type OrganizerId = OrganizerId of Guid

type Organizer = {
  Id : OrganizerId
  Firstname : string
  Lastname : string
}

type Voting =
  | Vote of AbstractId*OrganizerId
  | Veto of AbstractId*OrganizerId

let extractVoterId voting =
  match voting with
  | Vote (_,id) -> id
  | Veto (_,id) -> id

let extractAbstractId voting =
  match voting with
  | Vote (id,_) -> id
  | Veto (id,_) -> id

// type AcceptAbstract = proposedTalk -> AcceptedAbstract

// type Voter = Voting -> VotingResults -> VotingResults

type CallForPapers =
  | NotOpened
  | Open
  | Closed

type VotingPeriod =
  | InProgess
  | Finished

type ConferenceId = ConferenceId of Guid

type MaxVotesPerOrganizer = MaxVotesPerOrganizer of int
type MaxVetosPerOrganizer = MaxVetosPerOrganizer of int

type Conference = {
  Id : ConferenceId
  CallForPapers : CallForPapers
  VotingPeriod : VotingPeriod
  Abstracts : ConferenceAbstract list
  Votings : Voting list
  Organizers : Organizer list
  MaxVotesPerOrganizer : MaxVotesPerOrganizer
  MaxVetosPerOrganizer : MaxVetosPerOrganizer
  AvailableSlotsForTalks : int
}
