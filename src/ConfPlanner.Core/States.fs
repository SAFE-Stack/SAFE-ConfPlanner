module States

open Domain
open Events

type State =
  Conference

// Events kommen an, also hier keine Validierung etc mehr

let apply (state : State) event : State =
  match event with
    | AbstractWasProposed proposed ->
        { state with Abstracts = proposed :: state.Abstracts }

    | AbstractWasAccepted _ ->
        state

    | AbstractWasRejected _ ->
        state

    | VotingPeriodWasFinished ->
        { state with VotingPeriod = Finished }

    | VotingWasIssued voting ->
         { state with VotingResults = voting :: state.VotingResults }

    | VotingWasRevoked voting ->
         { state with VotingResults = state.VotingResults |> List.filter (fun v -> voting <> v) }




