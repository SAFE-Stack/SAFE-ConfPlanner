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

type ProposedAbstract =
  | Talk of AbstractData
  | HandsOn of AbstractData

type AcceptedAbstract =
  | Talk of AbstractData
  | HandsOn of AbstractData

type RejectedAbstract =
  | Talk of AbstractData
  | HandsOn of AbstractData

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

let extractVoterId voting =
  match voting with
  | Vote (_,id) -> id
  | Veto (_,id) -> id


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

type MaxVotesPerOrganizer = MaxVotesPerOrganizer of int

type Conference = {
  Id : ConferenceId
  CallForPapers : CallForPapers
  VotingPeriod : VotingPeriod
  ProposedAbstracts : ProposedAbstract list
  AcceptedAbstracts : AcceptedAbstract list
  RejectedAbstracts : RejectedAbstract list
  VotingResults : VotingResults
  Organizers : Organizer list
  MaxVotesPerOrganizer : MaxVotesPerOrganizer
}
