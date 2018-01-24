module Domain.Events

open Model

type Error =
  | PersonAlreadyRegistered of PersonId
  | ConferenceAlreadyScheduled
  | OrganizerAlreadyAddedToConference of PersonId
  | OrganizerWasNotAddedToConference of PersonId
  | OrganizerNotRegisteredInSystem of PersonId
  | Organizer
  | RevocationOfVotingWasDenied of Voting * error : string
  | FinishingDenied of string
  | VotingDenied of string
  | ProposingDenied of string

type Event =
  | PersonRegistered of Person
  | ConferenceScheduled of Conference
  | OrganizerAddedToConference of PersonId
  | OrganizerRemovedFromConference of PersonId
  | TalkWasProposed of ConferenceAbstract
  | CallForPapersOpened
  | CallForPapersClosed
  | TitleChanged of string
  | NumberOfSlotsDecided of int
  | VotingWasIssued of Voting
  | VotingWasRevoked of Voting
  | VotingPeriodWasFinished
  | VotingPeriodWasReopened
  | AbstractWasProposed of ConferenceAbstract
  | AbstractWasAccepted of AbstractId
  | AbstractWasRejected of AbstractId
  | Error of Error

let toString event =
  match event with
  | PersonRegistered person ->
      sprintf "Person Registered: %A" person

  | ConferenceScheduled conference ->
      sprintf "Conference scheduled: %A" conference

  | OrganizerAddedToConference organizer ->
      sprintf "The organizer %A was added to the conference" organizer

  | OrganizerRemovedFromConference organizer ->
      sprintf "The organizer %A was removed from the conference" organizer

  | TalkWasProposed conferenceAbstract ->
      sprintf "TalkWasProposed %A" conferenceAbstract

  | CallForPapersOpened ->
      "Call for papers was opened"

  | CallForPapersClosed ->
      "Call for papers was closed"

  | TitleChanged title ->
      sprintf "The title of the conference was changed to %s" title

  | NumberOfSlotsDecided number ->
      sprintf "The number of slots where changed to %i" number

  | VotingWasIssued (Voting (_,_,points)) ->
      sprintf "Voted: %s" (points |> pointsToString)

  | VotingWasRevoked voting ->
       sprintf "Voting was revoked: %A" voting

  | VotingPeriodWasFinished ->
      "Voting period was finished"

  | VotingPeriodWasReopened ->
      "Voting period was reopened"

  | AbstractWasProposed conferenceAbstract ->
      sprintf "AbstractWasProposed %A" conferenceAbstract

  | AbstractWasAccepted abstractId ->
      sprintf "AbstractWasAccepted %A" abstractId

  | AbstractWasRejected abstractId ->
      sprintf "AbstractWasRejected %A" abstractId

  | Error error ->
      sprintf "Error: %A" error

