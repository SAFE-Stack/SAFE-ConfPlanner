module Support.Helper

open System
open Infrastructure.Types
open Events
open Model


let transactionId () =
  TransactionId <| Guid.NewGuid()

let makeEventSets streamId events : EventSet<Event> list =
  events
  |> List.map (fun event -> (transactionId(), streamId), [event])

let makeStreamId (ConferenceId id) =
  id |> string |> StreamId

let voteTwo (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Voting (abstr.Id,organizer.Id, Two)

let voteOne (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Voting (abstr.Id,organizer.Id, One)

let voteZero (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Voting (abstr.Id,organizer.Id, Zero)

let veto (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Voting (abstr.Id,organizer.Id, Veto)

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
