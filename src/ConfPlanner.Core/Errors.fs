module Errors

open Domain

type Error =
  | CallForPapersNotOpened
  | CallForPapersClosed
  | CallForPapersNotClosed
  | VotingPeriodAlreadyFinished
  | VotingAlreadyIssued
  | VoterIsNotAnOrganizer
  | OrganizerDidNotVoteForAbstract
  | MaxNumberOfVotesExceeded
  | MaxNumberOfVetosExceeded

let toErrorString = function
| CallForPapersNotOpened -> "Call For Papers Not Opened"
| CallForPapersClosed -> "Call For Papers Closed"
| CallForPapersNotClosed -> "Call For Papers Not Closed"
| VotingPeriodAlreadyFinished -> "Voting Period Already Finished"
| VotingAlreadyIssued -> "Voting Already Issued"
| VoterIsNotAnOrganizer -> "Voter Is Not An Organizer"
| OrganizerDidNotVoteForAbstract -> "Organizer Did Not Vote For Abstract"
| MaxNumberOfVotesExceeded -> "Max Number Of Votes Exceeded"
| MaxNumberOfVetosExceeded -> "Max Number Of Vetos Exceeded"
