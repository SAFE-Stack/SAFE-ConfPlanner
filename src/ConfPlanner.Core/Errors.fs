module Errors

open Domain

type Error =
  | CallForPapersNotOpened
  | CallForPapersClosed
  | VotingPeriodAlreadyFinished

let toErrorString = function
| CallForPapersNotOpened -> "Call For Papers Not Opened"
| CallForPapersClosed -> "Call For Papers Closed"
| VotingPeriodAlreadyFinished -> "Voting Period Already Finished"
