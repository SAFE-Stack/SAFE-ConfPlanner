module Support.Helper

open System
open Infrastructure.Types
open Domain.Events
open Domain.Model


let transactionId () =
  TransactionId <| Guid.NewGuid()

let makeEventSets streamId events : EventSet<Event> list =
  events
  |> List.map (fun event -> (transactionId(), streamId), [event])

let makeStreamId (ConferenceId id) =
  id |> string |> StreamId

let voteTwo (abstr: ConferenceAbstract) organizer =
   Voting.Voting (abstr.Id, organizer, Two)

let voteOne (abstr: ConferenceAbstract) organizer =
   Voting.Voting (abstr.Id, organizer, One)

let voteZero (abstr: ConferenceAbstract) organizer =
   Voting.Voting (abstr.Id, organizer, Zero)

let veto (abstr: ConferenceAbstract) organizer =
   Voting.Voting (abstr.Id, organizer, Veto)

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
