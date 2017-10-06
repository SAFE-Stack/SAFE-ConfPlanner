open System
open Infrastructure.Types
open Infrastructure.EventStore
open Events
open Model

let getAllEvents,(storeEvents : Subscriber<EventSet<Event>>) =
  eventStore @"..\Server\conference_eventstore.json"

let transactionId () =
  TransactionId <| Guid.NewGuid()

let makeEventSets events =
  events
  |> List.map (fun event -> transactionId(), [event])


let heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
let fellien = { Firstname = "Janek";  Lastname = "Felien"; Id = OrganizerId <| Guid.NewGuid() }
let poepke = { Firstname = "Conrad";  Lastname = "Poepke"; Id = OrganizerId <| Guid.NewGuid() }

let proposedTalk title =
   {
      Id = AbstractId <| Guid.NewGuid()
      Duration = 45.
      Speakers = []
      Text = title
      Status = Proposed
      Type = Talk
   }

let talk1 = proposedTalk "Keynote: Modelling with Events"
let talk2 = proposedTalk "Talk: Keep your F#nctions pure"
let talk3 = proposedTalk "Talk: Elm is quite ok, I guess"

let vote (abstr: ConferenceAbstract) (organizer: Organizer) (value: Points) =
   Voting.Vote (abstr.Id,organizer.Id, value)

let veto (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Veto (abstr.Id,organizer.Id)

let events =
  [
    OrganizerRegistered heimeshoff
    OrganizerRegistered fellien
    OrganizerRegistered poepke
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (vote talk3 heimeshoff Points.Two)
    VotingWasIssued (veto talk3 fellien)
    VotingWasIssued (vote talk3 poepke Points.Two)
    VotingWasIssued (vote talk2 heimeshoff Points.One)
    VotingWasIssued (vote talk2 fellien Points.One)
    VotingWasIssued (vote talk2 poepke Points.One)
    VotingWasIssued (vote talk1 heimeshoff Points.Zero)
    VotingWasIssued (vote talk1 fellien Points.Zero)
    VotingWasIssued (vote talk1 poepke Points.Zero)
  ]


[<EntryPoint>]
let main argv =
    events
    |> makeEventSets
    |> List.iter storeEvents

    (*
      dirty hack to wait for all storeEvents calls to finish

      I need to change it to
      events
      |> makeEventSets
      |> List.map (fun eventSet -> async { do! storeEvents eventSet})
      |> Async.Parallel
      |> Async.RunSynchronously
      |> ignore

      but for this I need to change the type Subscriber<'a> to 'a -> Async<unit>
      and this needs to be implemented everywhere
    *)
    printfn "allEvents %A" <| getAllEvents()

    0
