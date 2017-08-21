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
  MaxVotesPerOrganizer = MaxVotesPerOrganizer 3
  MaxVetosPerOrganizer = MaxVetosPerOrganizer 1
}

let organizer1 = {
  Id = OrganizerId <| Guid.NewGuid()
  Firstname = "Roman"
  Lastname = "Sachse"
}

let abstractData() =
  {
  Id = AbstractId <| Guid.NewGuid()
  Duration = 45.
  Speakers = []
  Text = "bla"
  }

let proposedAbstract() =
  abstractData()
  |> ProposedAbstract.Talk

let withFinishedVotingPeriod conference =
  { conference with VotingPeriod = Finished }

let withCallForPapersNotOpened conference =
  { conference with CallForPapers = NotOpened }

let withCallForPapersOpen conference =
  { conference with CallForPapers = Open }

let withCallForPapersClosed conference =
  { conference with CallForPapers = Closed }

let withVotingPeriodInProgress conference =
  { conference with VotingPeriod = InProgess }

let withVotingPeriodFinished conference =
  { conference with VotingPeriod = Finished }

let withOrganizer organizer conference =
  { conference with Organizers = organizer :: conference.Organizers }

let withProposedAbstract proposedAbstract conference =
  { conference with ProposedAbstracts = proposedAbstract :: conference.ProposedAbstracts }

let withVoting voting conference =
  { conference with VotingResults = voting :: conference.VotingResults }

let withMaxVotesPerOrganizer max conference =
  { conference with MaxVotesPerOrganizer = MaxVotesPerOrganizer max }

let withMaxVetosPerOrganizer max conference =
  { conference with MaxVetosPerOrganizer = MaxVetosPerOrganizer max }
