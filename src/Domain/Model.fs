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

type Points = Points of int

type OrganizerId = OrganizerId of Guid

type Organizer = {
  Id : OrganizerId
  Firstname : string
  Lastname : string
}

type Voting =
  | Vote of AbstractId*OrganizerId*Points
  | Veto of AbstractId*OrganizerId

let extractVoterId voting =
  match voting with
  | Vote (_,id, _) -> id
  | Veto (_,id) -> id

let extractAbstractId voting =
  match voting with
  | Vote (id,_,_) -> id
  | Veto (id,_) -> id

let extractPoints voting =
  match voting with
  | Vote (id,_,points) -> (id, points)
  | Veto (_) -> failwith "Veto does not have points"

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

type Conference = {
  Id : ConferenceId
  CallForPapers : CallForPapers
  VotingPeriod : VotingPeriod
  Abstracts : ConferenceAbstract list
  Votings : Voting list
  Organizers : Organizer list
  AvailableSlotsForTalks : int
}
