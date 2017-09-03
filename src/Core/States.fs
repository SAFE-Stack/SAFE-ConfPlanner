module States

open Domain
open Events

type State =
  Conference

let updateAbstractStatus abstractId status (abstr: ConferenceAbstract) =
    match abstr.Id = abstractId with
    | true -> { abstr with Status = status }
    | false -> abstr


let apply (state : State) event : State =
  match event with
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

    | FinishingDenied(_) -> state

    | VotingDenied(_) -> state

    | ProposingDenied(_) -> state