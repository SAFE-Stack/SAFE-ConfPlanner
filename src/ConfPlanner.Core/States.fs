module States

open Domain
open Events

type State =
  Conference

// Events kommen an, also hier keine Validierung etc mehr

let apply (state : State) event : State =

  match event with
    VotingPeriodWasFinished ->
      match state.VotingPeriod with
      | InProgess ->
          { state with VotingPeriod = Finished }
      | Finished ->
          state

