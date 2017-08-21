module States

open Domain
open Events

type State =
  Conference

// Events kommen an, also hier keine Validierung etc mehr

let apply (state : State) event : State =
  match event with
    | AbstractWasProposed proposed ->
        { state with ProposedAbstracts = proposed :: state.ProposedAbstracts }

    | VotingPeriodWasFinished ->
        { state with VotingPeriod = Finished }

    | VotingWasIssued voting ->
         { state with VotingResults = voting :: state.VotingResults }




