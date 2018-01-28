module Domain.Behaviour

open Model
open Commands
open Events
open Projections

let (|OrganizerAlreadyInConference|_|) organizers organizer =
  match organizers |> List.contains organizer with
  | true -> Some organizer
  | false -> None

let (|OrganizerNotInConference|_|) organizers organizer =
  match organizers |> List.contains organizer with
  | false -> Some organizer
  | true -> None

let (|AlreadyVotedForAbstract|_|) votingResults voting =
  match List.contains voting votingResults with
  | true -> Some voting
  | false -> None

let (|DidNotVoteForAbstract|_|) votingResults voting =
  match not <| List.contains voting votingResults with
  | true -> Some voting
  | false -> None

let (|VoterIsNotAnOrganizer|_|) (organizers: Organizers) (voting : Voting) =
  let isNotOrganizer voting =
    organizers
    |> List.contains (extractVoterId voting)
    |> not

  match isNotOrganizer voting with
  | true -> Some voting
  | false -> None

let (|VotingisNotIssued|_|) (votings: Voting list) (voting : Voting) =
  let isIssued voting =
    votings |> List.contains voting

  match isIssued voting with
  | true -> None
  | false -> Some voting


let numberOfVotesExceeded votingResults getVote (voting : Voting) max =
  let number =
    votingResults
    |> List.filter (getVote <| extractVoterId voting)
    |> List.length

  match number >= max with
  | true -> Some voting
  | false -> None

let handleProposeAbstract givenHistory proposed =
  match (Conference.state givenHistory).CallForPapers with
  | Open ->
      [AbstractWasProposed proposed]

  | NotOpened ->
      [ProposingDenied "Call For Papers Not Opened" |> Error]

  | Closed ->
      [ProposingDenied "Call For Papers Closed" |> Error]

let score m (abstr : AbstractId) =
    match m |> Map.tryFind abstr with
    | Some value -> m |> Map.add abstr (value + 1)
    | None -> m |> Map.add abstr 1

let scoreAbstracts state =
  let talks,_ =
    state.Abstracts
    |> List.partition (fun abstr -> abstr.Type = Talk)

  let votes,vetos =
    state.Votings
    |> List.partition (function | Voting.Voting (_,_,Veto) -> false | _ -> true)

  let abstractsWithVetos =
    vetos
    |> List.map extractAbstractId

  let withoutVetos =
    votes
    |> List.map extractAbstractId
    |> List.filter (fun abstractId -> abstractsWithVetos |> List.contains abstractId |> not)

  let sumPoints abstractId =
    votes
      |> List.filter (fun voting -> voting |> extractAbstractId = abstractId)
      |> List.map extractPoints
      |> List.sumBy (function | Zero -> 0 | One -> 1 | Two -> 2 | Veto -> 0)

  let accepted =
    withoutVetos
    |> Seq.sortByDescending sumPoints
    |> Seq.distinct
    |> Seq.truncate state.AvailableSlotsForTalks
    |> Seq.toList

  let rejected =
    talks
    |> List.map (fun abstr -> abstr.Id)
    |> List.filter (fun id -> accepted |> List.contains id |> not)

  accepted
  |> List.map AbstractWasAccepted
  |> (@) (rejected |> List.map AbstractWasRejected)

let finishVotingPeriod conference =
  match conference.CallForPapers,conference.VotingPeriod with
  | Closed,InProgress ->
      let unfinishedVotings =
        conference.Abstracts
        |> Seq.map (fun abstr ->
            conference.Votings
            |> List.map extractAbstractId
            |> List.filter (fun id -> id = abstr.Id)
            |> List.length)
        |> Seq.filter (fun votes ->
            votes <> conference.Organizers.Length)
        |> Seq.length
      let events =
        match unfinishedVotings with
        | 0 ->
            [VotingPeriodWasFinished]
            |> (@) (scoreAbstracts conference)
        | _ -> [FinishingDenied "Not all abstracts have been voted for by all organisers" |> Error]
      events

  | Closed,Finished -> [FinishingDenied "Voting Period Already Finished" |> Error]

  | _,_ -> [FinishingDenied "Call For Papers Not Closed" |> Error]

let handleFinishVotingPeriod givenHistory =
  givenHistory
  |> Conference.state
  |> finishVotingPeriod

let reopenVotingPeriod conference =
  match conference.CallForPapers,conference.VotingPeriod with
  | Closed,Finished ->
      [VotingPeriodWasReopened]

  | _,_ ->
    [FinishingDenied "Call For Papers Not Closed" |> Error]

let handleReopenVotingPeriod givenHistory =
  givenHistory
  |> Conference.state
  |> reopenVotingPeriod

let vote voting conference =
  match conference.VotingPeriod with
  | Finished ->
      [VotingDenied "Voting Period Already Finished"|> Error]

  | InProgress ->
      match voting with
      | VoterIsNotAnOrganizer conference.Organizers _ ->
          [VotingDenied "Voter Is Not An Organizer" |> Error]

      | _ -> [VotingWasIssued voting]


let handleVote givenHistory voting =
  givenHistory
  |> Conference.state
  |> vote voting

let revokeVoting voting conference =
  match conference.VotingPeriod with
  | Finished ->
      [ RevocationOfVotingWasDenied (voting,"Voting Period Already Finished") |> Error ]

  | InProgress ->
      match voting with
      | VotingisNotIssued conference.Votings _ ->
          [ RevocationOfVotingWasDenied (voting,"Voting Not Issued") |> Error ]

      | _ -> [ VotingWasRevoked voting ]

let changeTitle title _ =
  [ TitleChanged title ]

let handleChangeTitle givenHistory title =
  givenHistory
  |> Conference.state
  |> changeTitle title

let decideNumberOfSlots number _ =
  [ NumberOfSlotsDecided number ]

let handleDecideNumberOfSlots givenHistory number =
  givenHistory
  |> Conference.state
  |> decideNumberOfSlots number

let handleRevokeVoting givenHistory voting =
  givenHistory
  |> Conference.state
  |> revokeVoting voting

let handleScheduleConference givenHistory conference =
  if givenHistory |> List.isEmpty then
    [ConferenceScheduled conference]
  else
    [ConferenceAlreadyScheduled |> Error]

let addOrganizerToConference organizer conference =
  match organizer with
  | OrganizerAlreadyInConference conference.Organizers _ ->
      [OrganizerAlreadyAddedToConference organizer |> Error]

  | _ -> [OrganizerAddedToConference organizer]

let private handleAddOrganizerToConference givenHistory organizer =
  givenHistory
  |> Conference.state
  |> addOrganizerToConference organizer

let removeOrganizerFromConference organizer conference =
  match organizer with
  | OrganizerNotInConference conference.Organizers _ ->
      [OrganizerWasNotAddedToConference organizer |> Error]

  | _ ->
    let revocations =
      conference.Votings
      |> votesOfOrganizer organizer
      |> List.map VotingWasRevoked

    [OrganizerRemovedFromConference organizer] @ revocations

let private handleRemoveOrganizerFromConference givenHistory organizer =
  givenHistory
  |> Conference.state
  |> removeOrganizerFromConference organizer

let private handleRegisterPerson givenHistory person =
  if givenHistory |> List.isEmpty then
    [PersonRegistered person]
  else
    [person.Id |> PersonAlreadyRegistered |> Error]

let execute (givenHistory : Event list) (command : Command) : Event list =
  match command with
  | RegisterPerson person ->
       person |> handleRegisterPerson givenHistory

  | ScheduleConference conference ->
      conference |> handleScheduleConference givenHistory

  | ChangeTitle title ->
      handleChangeTitle givenHistory title

  | DecideNumberOfSlots number ->
      handleDecideNumberOfSlots givenHistory number

  | AddOrganizerToConference organizer ->
      handleAddOrganizerToConference givenHistory organizer

  | RemoveOrganizerFromConference organizer ->
      handleRemoveOrganizerFromConference givenHistory organizer

  | ProposeAbstract proposed ->
      handleProposeAbstract givenHistory proposed

  | FinishVotingPeriod ->
      handleFinishVotingPeriod givenHistory

  | ReopenVotingPeriod ->
      handleReopenVotingPeriod givenHistory

  | Vote voting ->
      handleVote givenHistory voting

  | RevokeVoting voting  ->
      handleRevokeVoting givenHistory voting

  | AcceptAbstract(_) -> failwith "Not Implemented"

  | RejectAbstract(_) -> failwith "Not Implemented"

