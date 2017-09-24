module VoteTest

open System
open NUnit.Framework

open Model
open Commands
open Events
open Testbase


[<Test>]
let ``Can not vote when voting period is already finished`` () =
  let heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
  let talk = proposedTalk()
  let vote = vote talk heimeshoff Points.One

  Given [ 
    OrganizerRegistered heimeshoff 
    TalkWasProposed talk 
    VotingPeriodWasFinished]
  |> When (Vote vote)
  |> ThenExpect [VotingDenied "Voting Period Already Finished"]


[<Test>]
let ``Can not vote when voter is not organizer of conference`` () =
  let heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
  let talk = proposedTalk()
  let vote = vote talk heimeshoff Points.One

  Given [
    TalkWasProposed talk]
  |> When (Vote vote)
  |> ThenExpect [VotingDenied "Voter Is Not An Organizer"]


[<Test>]
let ``Can vote when constraints are fulfilled`` () =
  let heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
  let talk = proposedTalk()
  let vote = vote talk heimeshoff Points.One

  Given [ 
    OrganizerRegistered heimeshoff 
    TalkWasProposed talk]
  |> When (Vote vote)
  |> ThenExpect [VotingWasIssued vote]


[<Test>]
let ``Voter can change previous vote for an abstract`` () =
  let heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
  let talk = proposedTalk()
  let vote = vote talk heimeshoff Points.One

  Given [ 
    OrganizerRegistered heimeshoff 
    TalkWasProposed talk 
    VotingWasIssued vote]
  |> When (Vote vote)
  |> ThenExpect [VotingWasIssued vote]


[<Test>]
let ``Can issue a veto when constraints are fulfilled`` () =
  let heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
  let talk = proposedTalk()
  let veto= veto talk heimeshoff

  Given [ 
    OrganizerRegistered heimeshoff 
    TalkWasProposed talk]
  |> When (Vote veto)
  |> ThenExpect [VotingWasIssued veto]