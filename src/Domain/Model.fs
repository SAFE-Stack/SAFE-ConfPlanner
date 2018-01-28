module Domain.Model

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

let pointsToString points =
  match points with
  | Zero -> "0"
  | One -> "1"
  | Two -> "2"
  | Veto -> "veto"

type PersonId =
  PersonId of Guid

type Person = {
  Id : PersonId
  Firstname : string
  Lastname : string
}

let emptyPerson() = {
  Id = System.Guid.NewGuid() |> PersonId
  Firstname = ""
  Lastname = ""
}


type Organizers =
  PersonId list

let person firstname lastname guid =
  {
    Id = PersonId <| Guid.Parse guid
    Firstname = firstname
    Lastname = lastname
  }

type Voting =
  Voting of AbstractId * PersonId * Points

let extractAbstractId (Voting (id,_,_)) =
  id

let extractVoterId  (Voting (_,id,_)) =
  id

let extractPoints (Voting (_,_,points)) =
  points

let extractVoteForAbstract voterId abstractId votings =
  let vote =
    votings
    |> List.filter (fun voting -> voting |> extractAbstractId = abstractId && voting |> extractVoterId = voterId)

  match vote with
  | [vote] ->
      vote |> Some

  | _ ->
      None

let votesOfOrganizer personId votings =
  votings
  |> List.filter (fun voting -> voting |> extractVoterId = personId)


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
  Organizers : Organizers
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

let withTitle title conference =
  { conference with Title = title }

let withAvailableSlotsForTalks availableSlotsForTalks conference =
  { conference with AvailableSlotsForTalks = availableSlotsForTalks }
