module Support.Helper

open System
open Domain.Model

let voteTwo (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting (abstr.Id,organizer.Id, Two)

let voteOne (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting (abstr.Id,organizer.Id, One)

let voteZero (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting (abstr.Id,organizer.Id, Zero)

let veto (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting (abstr.Id,organizer.Id, Veto)

let proposedTalk title firstname lastname =
  let speaker =
    {
      Speaker.Id = SpeakerId <| Guid.NewGuid()
      Firstname = firstname
      Lastname = lastname
    }
  {
    Id = AbstractId <| Guid.NewGuid()
    Duration = 45.
    Speakers = [speaker]
    Text = title
    Status = Proposed
    Type = Talk
  }
