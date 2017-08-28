module States

open Domain
open Events

type State =
  Conference

// Events kommen an, also hier keine Validierung etc mehr

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
         { state with VotingResults = voting :: state.VotingResults }

    | VotingWasRevoked voting ->
         { state with VotingResults = state.VotingResults |> List.filter (fun v -> voting <> v) }




