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

let votesOfOrganizer organizerId votings =
  votings
  |> List.filter (fun voting -> voting |> extractVoterId = organizerId)


type CallForPapers =
  | NotOpened
  | Open
  | Closed

type VotingPeriod =
  | InProgress
  | Finished

type AttendeeId = AttendeeId of Guid

type AdminId = AdminId of System.Guid

type Role =
  | Admin of AdminId
  | Organizer of OrganizerId
  | Attendee of AttendeeId


type Identity = Identity of System.Guid

[<RequireQualifiedAccessAttribute>]
module Roles =
  type Container =
    {
      Admin : AdminId option
      Organizer : OrganizerId option
      Attendee : AttendeeId option
    }

  let empty =
    {
      Admin = None
      Organizer = None
      Attendee = None
    }

  let withAdmin admin roles =
    { roles with Admin = Some admin }

  let withOrganizer organizer roles =
    { roles with Organizer = Some organizer }

  let withAttendee attendee roles =
    { roles with Attendee = Some attendee }

  let private concatX b a get set =
    match get a, get b with
    | None, Some x ->
        a |> set x

    | _ -> a

  let private concatAdmin b a =
    concatX b a (fun x -> x.Admin) withAdmin

  let private concatOrganizer a b =
     concatX b a (fun x -> x.Organizer) withOrganizer

  let private concatAttendee a b =
     concatX b a (fun x -> x.Attendee) withAttendee

  let private concatTwo a b =
    a
    |> concatAdmin b
    |> concatAttendee b
    |> concatOrganizer b

  let concat list =
    list |> List.reduce concatTwo


type UserId = UserId of Guid

type User =
  {
    Id : UserId
    Roles : Role list
  }

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

let withTitle title conference =
  { conference with Title = title }

let withAvailableSlotsForTalks availableSlotsForTalks conference =
  { conference with AvailableSlotsForTalks = availableSlotsForTalks }
