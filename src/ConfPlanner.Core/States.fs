module States

open Domain
open Events

type State =
  Conference

// Events kommen an, also hier keine Validierung etc mehr

let apply (state : State) event : State =
  match event with
    | AbstractWasProposed proposed->
        match state.CallForPapers with
        | Open ->
            { state with ProposedAbstracts = proposed :: state.ProposedAbstracts }
        | _ ->
            state
    | VotingPeriodWasFinished ->
      match state.VotingPeriod with
      | InProgess ->
          { state with VotingPeriod = Finished }
      | Finished ->
          state



