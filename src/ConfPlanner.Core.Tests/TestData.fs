module TestData
open Domain
open System

let conference = {
  Id = ConferenceId <| Guid.NewGuid()
  CallForPapers = NotOpened
  VotingPeriod = InProgess
  ProposedAbstracts = []
  AcceptedAbstracts = []
  RejectedAbstracts = []
  VotingResults = []
  Organizers = []
}


let abstractData =
  {
  Id = AbstractId <| Guid.NewGuid()
  Duration = 45.
  Speakers = []
  Text = "bla"
  }

let proposedAbstract =
  abstractData
  |> ProposedAbstract.Talk

let withFinishedVotingPeriod conference =
  { conference with VotingPeriod = Finished }

let withCallForPapersNotOpened conference =
  { conference with CallForPapers = NotOpened }

let withCallForPapersOpen conference =
  { conference with CallForPapers = Open }

let withCallForPapersClosed conference =
  { conference with CallForPapers = Closed }