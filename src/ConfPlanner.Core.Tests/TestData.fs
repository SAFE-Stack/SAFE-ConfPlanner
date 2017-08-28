module TestData
open Domain
open System

let conference = {
  Id = ConferenceId <| Guid.NewGuid()
  CallForPapers = NotOpened
  VotingPeriod = InProgess
  Abstracts = []
  Votings = []
  Organizers = []
  MaxVotesPerOrganizer = MaxVotesPerOrganizer 3
  MaxVetosPerOrganizer = MaxVetosPerOrganizer 1
  AvailableSlotsForTalks = 2
}

let organizer() = {
  Id = OrganizerId <| Guid.NewGuid()
  Firstname = ""
  Lastname = ""
}

let proposedTalk() =
   {
      Id = AbstractId <| Guid.NewGuid()
      Duration = 45.
      Speakers = []
      Text = "bla"
      Status = Proposed
      Type = Talk
   }

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

let withOrganizers organizers conference =
  { conference with Organizers = organizers }

let withAbstract conferenceAbstract conference =
  { conference with Abstracts = conferenceAbstract :: conference.Abstracts }

let withAbstracts conferenceAbstracts conference =
  { conference with Abstracts = conferenceAbstracts }

let withVoting voting conference =
  { conference with Votings = voting :: conference.Votings }

let withVotings votings conference =
  { conference with Votings = votings }

let withMaxVotesPerOrganizer max conference =
  { conference with MaxVotesPerOrganizer = MaxVotesPerOrganizer max }

let withMaxVetosPerOrganizer max conference =
  { conference with MaxVetosPerOrganizer = MaxVetosPerOrganizer max }

let withAvailableSlotsForTalks number conference =
  { conference with AvailableSlotsForTalks = number }

let accepted abstr =
  { abstr with Status = Accepted }

let rejected abstr =
   { abstr with Status = Rejected }

let vote (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Vote (abstr.Id,organizer.Id)

let veto (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Veto (abstr.Id,organizer.Id)

