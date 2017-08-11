module States

open Domain
open Events

type State =
  | CallForPapersOpen
  | CallForPapersClosed
  | VotingPeriodInProgress
  | VotingPeriodFinished


type State2 = {
  Voting
}

let apply state event =
  match state,event with
  | VotingPeriodInProgress, VotingWasIssued _ ->
      VotingPeriodInProgress