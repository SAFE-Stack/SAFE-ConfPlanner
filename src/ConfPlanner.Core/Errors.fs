module Errors

open Domain

type Error =
  | VotingPeriodAlreadyFinished

let toErrorString = function
| VotingPeriodAlreadyFinished -> "Voting Period Already Finished"
