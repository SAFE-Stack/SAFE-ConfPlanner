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

type ProposedAbstract =
  | Talk of AbstractData
  | HandsOnSession of AbstractData

type AcceptedAbstract =
  | Talk of AbstractData
  | HandsOnSession of AbstractData

type OrganizerId = OrganizerId of Guid

type Organizer = {
  Id : Organizer
  Firstname : string
  Lastname : string
}

type Voting =
  | Vote of ProposedAbstract*OrganizerId
  | Veto of ProposedAbstract*OrganizerId

type VotingResults = Voting list

type AcceptAbstract = ProposedAbstract -> AcceptedAbstract

type Voter = Voting -> VotingResults -> VotingResults

let voter voting results =
  voting :: results


type VotingService = {
  AcceptAbstract : AcceptAbstract
  Voter : Voter
}