module Domain.Events

open Model

type DomainError =
  | ConferenceAlreadyScheduled
  | OrganizerAlreadyAddedToConference of Organizer
  | OrganizerWasNotAddedToConference of Organizer
  | RevocationOfVotingWasDenied of Voting * error : string
  | FinishingDenied of string
  | VotingDenied of string
  | ProposingDenied of string

type Event =
  | ConferenceScheduled of Conference
  | OrganizerAddedToConference of Organizer
  | OrganizerRemovedFromConference of Organizer
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
  | DomainError of DomainError

  with
    static member ToString event =
      match event with
      | ConferenceScheduled conference ->
          sprintf "Conference scheduled: %A" conference

      | OrganizerAddedToConference organizer ->
          sprintf "The organizer %s %s was added to the conference" organizer.Firstname organizer.Lastname

      | OrganizerRemovedFromConference organizer ->
          sprintf "The organizer %s %s was removed from the conference" organizer.Firstname organizer.Lastname

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

      | DomainError error ->
          sprintf "Error: %A" error


    static member isAnError event =
      match event with
      | DomainError _ -> true
      | _ -> false





