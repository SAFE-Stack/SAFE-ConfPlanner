module TestData
open Domain
open System

let conference() = {
  Id = ConferenceId <| Guid.NewGuid()
  CallForPapers = Open
  VotingPeriod = InProgess
  Abstracts = []
  VotingResults = []
  Organizers = []
}

let withFinishedVotingPeriod conference =
  { conference with VotingPeriod = Finished }
