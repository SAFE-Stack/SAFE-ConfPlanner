module Errors

open Domain

type Error =
  | CallForPapersNotOpened
  | CallForPapersClosed
  | VotingPeriodAlreadyFinished
  | VotingAlreadyIssued
  | VoterIsNotAnOrganizer
  | MaxNumberOfVotesExceeded

let toErrorString = function
| CallForPapersNotOpened -> "Call For Papers Not Opened"
| CallForPapersClosed -> "Call For Papers Closed"
| VotingPeriodAlreadyFinished -> "Voting Period Already Finished"
| VotingAlreadyIssued -> "Voting Already Issued"
| VoterIsNotAnOrganizer -> "Voter Is Not An Organizer"
| MaxNumberOfVotesExceeded -> "Max Number Of Votes Exceeded"
