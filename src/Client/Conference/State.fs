module Conference.State

open Elmish
open Elmish.Helper
open Global

open Server.ServerTypes
open EventSourced

open Conference.Types
open Conference.Ws
open Domain
open Domain.Model
open App.Server
open Application

let sendCommand commandEnvelope : Async<Msg> =
  async {
    match! commandPort.Handle commandEnvelope with
    | Ok eventEnvelopes ->
        return CommandResponse (commandEnvelope.Transaction, Ok eventEnvelopes)

    | Result.Error error ->
        return CommandResponse (commandEnvelope.Transaction, Result.Error error)
  }

let private eventIsForConference (ConferenceId conferenceId) envelope =
  envelope.Metadata.Source = conferenceId

let private updateStateWithEvents conference events  =
  events
  |> List.fold Domain.Projections.evolve conference


let private queryConference conferenceId =
  // TODO react to query Error
  Cmd.OfAsync.perform conferenceApi.conference conferenceId ConferenceLoaded

let private queryConferences =
  Cmd.OfAsync.perform conferenceApi.conferences () ConferencesLoaded

let private queryOrganizers =
  Cmd.OfAsync.perform organizerApi.organizers () OrganizersLoaded

let init (user : UserData)  =
  {
    View = CurrentView.NotAsked
    Conferences = RemoteData.NotAsked
    Organizers = RemoteData.NotAsked
    LastEvents = None
    Organizer = user.OrganizerId
    OpenTransactions = Map.empty
    OpenNotifications = []
  }, Cmd.ofSub <| startWs user.Token

let dispose () =
  Cmd.ofSub stopWs

let private messageSendAfterMilliseconds timeout msg  =
  fun dispatch -> Browser.Dom.window.setTimeout((fun _ -> msg |> dispatch), timeout) |> ignore
  |> Cmd.ofSub

let private withView view model =
  { model with View = view }

let private withReceivedEvents eventEnvelopes model =
  { model with LastEvents = Some eventEnvelopes }
  |> withoutCmds

let withAdditionalOpenNotifications notifications model =
  { model with  OpenNotifications = model.OpenNotifications @ notifications }

let withRequestedForRemovalNotification (event,transaction,_) model =
  let mapper ((ev,tx,_) as notification) =
    if event = ev && transaction = tx then
      (event,tx,Leaving)
    else
      notification

  let cmd =
    (event,transaction,Leaving)
    |> RemoveNotification
    |> messageSendAfterMilliseconds 2000

  { model with OpenNotifications = model.OpenNotifications |> List.map mapper }
  |> withCommand cmd

let withoutNotification (notification,transaction,_) model =
  let newNotifications =
     model.OpenNotifications
     |> List.filter (fun (event,tx,_) -> (event = notification && tx = transaction) |> not )

  { model with OpenNotifications = newNotifications }

let private updateWhatIfView editor conference whatif command (behaviour : Conference -> Domain.Events.Event list) =
  let events =
    conference |> behaviour

  let newConference =
    events |> updateStateWithEvents conference

  let whatif =
    WhatIf
      {
        whatif with
          Events = events
          Commands = command :: whatif.Commands
      }

  Edit (editor, newConference, whatif)

let commandEnvelopeForMessage conferenceId msg =
  match msg with
  | Vote voting ->
      API.Command.conferenceApi.Vote voting conferenceId

  | RevokeVoting voting ->
      API.Command.conferenceApi.RevokeVoting voting conferenceId

  | FinishVotingperiod ->
      API.Command.conferenceApi.FinishVotingPeriod conferenceId

  | ReopenVotingperiod ->
      API.Command.conferenceApi.ReopenVotingPeriod conferenceId

  | AddOrganizerToConference organizer ->
      API.Command.conferenceApi.AddOrganizerToConference organizer conferenceId

  | RemoveOrganizerFromConference organizer ->
      API.Command.conferenceApi.RemoveOrganizerFromConference organizer conferenceId

  | ChangeTitle title ->
      API.Command.conferenceApi.ChangeTitle title conferenceId

  | DecideNumberOfSlots number ->
     API.Command.conferenceApi.DecideNumberOfSlots number conferenceId

let eventsForMessage msg =
  match msg with
  | Vote voting ->
      Behaviour.vote voting

  | RevokeVoting voting ->
      Behaviour.revokeVoting voting

  | FinishVotingperiod ->
      Behaviour.finishVotingPeriod

  | ReopenVotingperiod ->
      Behaviour.reopenVotingPeriod

  | AddOrganizerToConference organizer ->
      Behaviour.addOrganizerToConference organizer

  | RemoveOrganizerFromConference organizer ->
      Behaviour.removeOrganizerFromConference organizer

  | ChangeTitle title ->
      Behaviour.changeTitle title

  | DecideNumberOfSlots number ->
      Behaviour.decideNumberOfSlots number

let private updateWhatIf msg editor conference whatif =
  updateWhatIfView
    editor
    conference
    whatif
    (commandEnvelopeForMessage conference.Id msg)
    (eventsForMessage msg)

let private withWsCmd command conference model =
  let transaction =
    transactionId()

  let (ConferenceId eventSource) = conference.Id

  let envelope =
    {
      Transaction = transaction
      EventSource = eventSource
      Command = command
    }

  model
  |> withCommand (wsCmd (ClientMsg.Command envelope))

let private eventEnvelopeAsNewNotification eventEnvelope =
  eventEnvelope.Event,eventEnvelope.Metadata.Transaction,Entered

let private addedToOpenTransactions model transaction =
  { model with OpenTransactions = model.OpenTransactions |> Map.add transaction Deferred.InProgress }

let private withApiCommand commandEnvelope model =
  commandEnvelope.Transaction
  |> addedToOpenTransactions model
  |> withCommand (Cmd.fromAsync (sendCommand commandEnvelope))

let private withOpenCommands transactions model =
  transactions
  |> List.fold addedToOpenTransactions model

let withoutTransaction transaction model =
  { model with OpenTransactions = model.OpenTransactions |> Map.remove transaction }

let private makeItSo commandEnvelopes model =
  let cmds =
    commandEnvelopes
    |> List.rev
    |> List.collect (sendCommand >> Cmd.fromAsync)

  let model =
    model
    |> withOpenCommands (commandEnvelopes |> List.map (fun ee -> ee.Transaction))

  model,cmds


let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
  match msg with
  | OrganizersLoaded (Ok organizers) ->
      { model with Organizers = organizers |> RemoteData.Success }
      |> withoutCmds

  | OrganizersLoaded (Result.Error _) ->
      model |> withoutCmds

  | ConferencesLoaded (Ok conferences) ->
      { model with Conferences = conferences |> RemoteData.Success }
      |> withoutCmds

  | ConferencesLoaded (Result.Error _) ->
      model |> withoutCmds

  | ConferenceLoaded (Ok conference) ->
      model
      |> withView ((VotingPanel,conference,Live) |> Edit)
      |> withoutCmds

  | ConferenceLoaded (Result.Error _) ->
      model |> withoutCmds

  | Received (ServerMsg.Connected) ->
      model, Cmd.batch [ queryConferences ; queryOrganizers ]

  | Received (ServerMsg.Events events) ->
      match model.View with
      | Edit (editor, conference, Live) ->
          let newConference =
            events
            |> List.filter (eventIsForConference conference.Id)
            |> List.map (fun envelope -> envelope.Event)
            |> updateStateWithEvents conference

          model
          |> withView ((editor,newConference,Live) |> Edit)
          |> withReceivedEvents events

      | _ ->
          model |> withoutCmds

  | WhatIfMsg msg ->
      match model.View with
      | Edit (_, conference, Live) ->
          model
          |> withApiCommand (commandEnvelopeForMessage conference.Id msg)

      | Edit (editor, conference, WhatIf whatif) ->
          model
          |> withView (updateWhatIf msg editor conference whatif)
          |> withoutCmds

      | _ ->
           model |> withoutCmds

  | MakeItSo ->
      match model.View with
      | Edit (editor, conference, WhatIf whatIf)  ->
          let model,cmds =
            makeItSo whatIf.Commands model

          model
          |> withView (Edit (editor,whatIf.Conference,Live))
          |> withCommand (Cmd.batch [cmds ; queryConference conference.Id])

      | _ ->
          model |> withoutCmds

  | ToggleMode ->
      match model.View with
      | Edit (editor, conference, Live) ->
          let whatif =
            {
              Conference = conference
              Commands = []
              Events = []
            }

          model
          |> withView ((editor, conference, whatif |> WhatIf) |> Edit)
          |> withoutCmds

      | Edit (editor, conference, WhatIf _) ->
          { model with View = (editor, conference, Live) |> Edit },
          conference.Id |> queryConference

      | _ ->
          model |> withoutCmds

  | SwitchToConference conferenceId ->
      model, conferenceId |> queryConference

  | SwitchToEditor target ->
      match model.View with
      | Edit (_, conference, mode) ->
          let editor =
            match target with
            | AvailableEditor.ConferenceInformation ->
                ConferenceInformation.State.init conference.Title (conference.AvailableSlotsForTalks |> string)
                |> Editor.ConferenceInformation

            | AvailableEditor.VotingPanel ->
                Editor.VotingPanel

            | AvailableEditor.Organizers ->
                Editor.Organizers

          model
          |> withView ((editor, conference, mode) |> Edit)
          |> withoutCmds

      | _ ->
          model |> withoutCmds

  | SwitchToNewConference ->
      model
      |> withView (ConferenceInformation.State.init "" "" |> CurrentView.ScheduleNewConference)
      |> withoutCmds

  | ResetConferenceInformation ->
      match model.View with
      | Edit (ConferenceInformation _, conference, mode) ->
          let editor =
            ConferenceInformation.State.init conference.Title (conference.AvailableSlotsForTalks |> string)
            |> Editor.ConferenceInformation

          model
          |> withView ((editor, conference, mode) |> Edit)
          |> withoutCmds

      | _ ->
          model |> withoutCmds

  | UpdateConferenceInformation ->
      match model.View with
      | Edit (ConferenceInformation submodel, conference, _) when submodel |> ConferenceInformation.Types.isValid ->
          let title =
            submodel |> ConferenceInformation.Types.title

          let titleCmd =
            if title <> conference.Title then
              title
              |> ChangeTitle
              |> WhatIfMsg
              |> Cmd.ofMsg
            else
              Cmd.none

          let availableSlotsForTalks =
            submodel |> ConferenceInformation.Types.availableSlotsForTalks

          let availableSlotsForTalksCmd =
            if availableSlotsForTalks <> conference.AvailableSlotsForTalks then
              availableSlotsForTalks
              |> DecideNumberOfSlots
              |> WhatIfMsg
              |> Cmd.ofMsg
            else
              Cmd.none

          model
          |> withCommand (Cmd.batch [ titleCmd ; availableSlotsForTalksCmd ])

      | _ ->
          model |> withoutCmds

  | Msg.ScheduleNewConference ->
      match model.View with
      | ScheduleNewConference submodel when submodel |> ConferenceInformation.Types.isValid ->
          let title =
            submodel |> ConferenceInformation.Types.title

          let availableSlotsForTalks =
            submodel |> ConferenceInformation.Types.availableSlotsForTalks

          let conference =
            emptyConference()
            |> withTitle title
            |> withAvailableSlotsForTalks availableSlotsForTalks

          let command =
            conference |> Commands.ScheduleConference

          let editor =
            ConferenceInformation.State.init conference.Title (conference.AvailableSlotsForTalks |> string)
            |> Editor.ConferenceInformation

          model
          |> withView ((editor, conference, Live) |> Edit)
          |> withWsCmd command conference

      | _ ->
          model |> withoutCmds

  | ConferenceInformationMsg msg ->
      match model.View with
      | Edit (ConferenceInformation submodel, conference, mode) ->
          let newSubmodel =
            submodel |> ConferenceInformation.State.update msg

          model
          |> withView ((ConferenceInformation newSubmodel, conference, mode) |> Edit)
          |> withoutCmds

      | ScheduleNewConference submodel ->
          let view =
            submodel
            |> ConferenceInformation.State.update msg
            |> ScheduleNewConference

          model
          |> withView view
          |> withoutCmds

      | _ ->
          model |> withoutCmds

  | RequestNotificationForRemoval notification ->
      model
      |> withRequestedForRemovalNotification notification

  | RemoveNotification notification ->
      model
      |> withoutNotification notification
      |> withoutCmds

  | CommandResponse (transaction, Ok eventEnvelopes) ->
      let notifications =
        eventEnvelopes
        |> List.map eventEnvelopeAsNewNotification

      let cmds =
        notifications
        |> List.map (RequestNotificationForRemoval >> (messageSendAfterMilliseconds 5000))
        |> Cmd.batch

      model
      |> withAdditionalOpenNotifications notifications
      |> withoutTransaction transaction
      |> withCommand cmds

    | CommandResponse (transaction, Result.Error error) ->
          model |> withoutCmds
          // TODO: damit umgehen

