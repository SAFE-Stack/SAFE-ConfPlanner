module  Model

open System

type SpeakerId = SpeakerId of Guid

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

type Points =
  | Zero
  | One
  | Two
  | Veto

type OrganizerId = OrganizerId of Guid

type Organizer = {
  Id : OrganizerId
  Firstname : string
  Lastname : string
}

type Organizers =
  Organizer list

let organizer firstname lastname guid =
  {
    Id = OrganizerId <| Guid.Parse guid
    Firstname = firstname
    Lastname = lastname
  }

type Voting =
  Voting of AbstractId * OrganizerId * Points

let extractAbstractId (Voting (id,_,_)) =
  id

let extractVoterId  (Voting (_,id,_)) =
  id

let extractPoints (Voting (_,_,points)) =
  points

let extractVoteForAbstract organizerId abstractId votings =
  let vote =
    votings
    |> List.filter (fun voting -> voting |> extractAbstractId = abstractId && voting |> extractVoterId = organizerId)

  match vote with
  | [vote] ->
      vote |> Some

  | _ ->
      None


type CallForPapers =
  | NotOpened
  | Open
  | Closed

type VotingPeriod =
  | InProgress
  | Finished

type ConferenceId = ConferenceId of Guid

type Conference = {
  Id : ConferenceId
  Title : string
  CallForPapers : CallForPapers
  VotingPeriod : VotingPeriod
  Abstracts : ConferenceAbstract list
  Votings : Voting list
  Organizers : Organizer list
  AvailableSlotsForTalks : int
}

let emptyConference() = {
  Id = System.Guid.NewGuid() |> ConferenceId
  Title = ""
  CallForPapers = NotOpened
  VotingPeriod = InProgress
  Abstracts = []
  Votings = []
  Organizers = []
  AvailableSlotsForTalks = 2
}
