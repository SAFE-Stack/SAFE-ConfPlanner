module Domain.Projections

open System
open Model
open Events
open EventSourced

let updateAbstractStatus abstractId status (abstr: ConferenceAbstract) =
    match abstr.Id = abstractId with
    | true -> { abstr with Status = status }
    | false -> abstr

let evolve (conference : Conference) event : Conference =
  match event with
    | ConferenceScheduled conference ->
        conference

    | OrganizerAddedToConference o ->
        { conference with Organizers = o :: conference.Organizers }

    | OrganizerRemovedFromConference organizer ->
        let newOrganizers =
          conference.Organizers
          |> List.filter (fun o -> o.Id <> organizer.Id)

        { conference with Organizers = newOrganizers }

    | TalkWasProposed t ->
        { conference with Abstracts = t :: conference.Abstracts }

    | CallForPapersOpened ->
        { conference with CallForPapers = Open }

    | CallForPapersClosed ->
        { conference with
            CallForPapers = Closed
            VotingPeriod = InProgress }

    | TitleChanged title ->
        { conference with Title = title }

    | NumberOfSlotsDecided number ->
        { conference with AvailableSlotsForTalks = number }

    | AbstractWasProposed proposed ->
        { conference with Abstracts = proposed :: conference.Abstracts }

    | AbstractWasAccepted abstractId ->
        { conference with Abstracts = conference.Abstracts |> List.map (updateAbstractStatus abstractId AbstractStatus.Accepted) }

    | AbstractWasRejected abstractId ->
        { conference with Abstracts = conference.Abstracts |> List.map (updateAbstractStatus abstractId AbstractStatus.Rejected) }

    | VotingPeriodWasFinished ->
        { conference with VotingPeriod = Finished }

    | VotingPeriodWasReopened ->
        { conference with
            VotingPeriod = InProgress
            Abstracts = conference.Abstracts |> List.map (fun abstr -> { abstr with Status = AbstractStatus.Proposed }) }

    | VotingWasIssued (Voting.Voting (abstractId, organizerId, _) as voting) ->
        let votings =
          conference.Votings
          |> List.filter (fun voting -> voting |> extractAbstractId <> abstractId || voting |> extractVoterId <> organizerId)

        { conference with Votings = voting :: votings }

    | VotingWasRevoked (Voting.Voting (abstractId, organizerId, _)) ->
        let votings =
          conference.Votings
          |> List.filter (fun voting -> voting |> extractAbstractId <> abstractId || voting |> extractVoterId <> organizerId)

        { conference with Votings = votings }

    | DomainError _ ->
        conference

let private emptyConference : Conference =
  {
    Id = ConferenceId <| Guid.Empty
    Title = ""
    CallForPapers = NotOpened
    VotingPeriod = InProgress
    Abstracts = List.empty
    Votings = List.empty
    Organizers = List.empty
    AvailableSlotsForTalks = 0
  }

let conferenceState (givenHistory : Event list) =
    givenHistory
    |> List.fold evolve emptyConference


let conference : Projection<Conference, Event> =
  {
    Init = emptyConference
    Update = evolve
  }
