module Projections

open System
open Model
open Events

let updateAbstractStatus abstractId status (abstr: ConferenceAbstract) =
    match abstr.Id = abstractId with
    | true -> { abstr with Status = status }
    | false -> abstr


let apply (state : Conference) event : Conference =
  match event with
    | OrganizerRegistered o ->
        { state with Organizers = o :: state.Organizers }

    | TalkWasProposed t ->
        { state with Abstracts = t :: state.Abstracts }

    | CallForPapersOpened ->
        { state with CallForPapers = Open }

    | CallForPapersClosed ->
        { state with CallForPapers = Closed; VotingPeriod = InProgess }

    | NumberOfSlotsDecided i ->
        { state with AvailableSlotsForTalks = i }

    | AbstractWasProposed proposed ->
        { state with Abstracts = proposed :: state.Abstracts }

    | AbstractWasAccepted abstractId ->
        { state with Abstracts = state.Abstracts |> List.map (updateAbstractStatus abstractId AbstractStatus.Accepted) }

    | AbstractWasRejected abstractId ->
        { state with Abstracts = state.Abstracts |> List.map (updateAbstractStatus abstractId AbstractStatus.Rejected) }

    | VotingPeriodWasFinished ->
        { state with VotingPeriod = Finished }

    | VotingWasIssued voting ->
        { state with Votings = voting :: state.Votings }

    | FinishingDenied _ ->
        state

    | VotingDenied _ ->
        state

    | ProposingDenied _ ->
        state

let emptyConference : Conference =
  {
    Id = ConferenceId <| Guid.Empty
    CallForPapers = NotOpened
    VotingPeriod = InProgess
    Abstracts = List.empty
    Votings = List.empty
    Organizers = List.empty
    AvailableSlotsForTalks = 0
  }

let conferenceState (givenHistory: Event list) =
    givenHistory
    |> List.fold apply emptyConference
