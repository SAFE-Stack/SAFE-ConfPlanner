module Conference.State

open Elmish
open Utils
open Utils.Elmish
open Thoth.Elmish

open Server.ServerTypes
open EventSourced

open Conference.Types
open Conference.Ws
open Domain.Model
open Application

let private eventIsForConference (ConferenceId conferenceId) envelope =
  envelope.Metadata.Source = conferenceId

let private withView view model =
  { model with View = view }

let private withReceivedEvents eventEnvelopes model =
  { model with LastEvents = eventEnvelopes }
  |> withoutCmds

let private updateWhatIfView editor conference whatif command (behaviour : Conference -> Domain.Events.Event list) =
  let events =
    conference |> behaviour

  let newConference =
    events |> Api.Local.evolve conference

  let whatif =
    WhatIf
      {
        whatif with
          Events = events
          Commands = whatif.Commands @ [ command ]
      }

  Edit (editor, newConference, whatif)

let private addedToOpenTransactions model transaction =
  { model with OpenTransactions = model.OpenTransactions |> Map.add transaction Deferred.InProgress }

let private withApiCommand commandEnvelope model =
  commandEnvelope.Transaction
  |> addedToOpenTransactions model
  |> withCmd (Cmd.fromAsync (Api.sendCommand commandEnvelope))

let private withOpenCommands transactions model =
  transactions
  |> List.fold addedToOpenTransactions model

let private withoutTransaction transaction model =
  { model with OpenTransactions = model.OpenTransactions |> Map.remove transaction }

let private makeItSo commandEnvelopes model =
  let cmds =
    commandEnvelopes
    |> List.collect (Api.sendCommand >> Cmd.fromAsync)

  let model =
    model
    |> withOpenCommands (commandEnvelopes |> List.map (fun ee -> ee.Transaction))

  model,cmds

let init (user : UserData)  =
  {
    View = NotAsked
    Conferences = HasNotStartedYet
    Organizers = HasNotStartedYet
    LastEvents = []
    Organizer = user.OrganizerId
    OpenTransactions = Map.empty
  }, Cmd.ofSub <| startWs user.Token

let dispose () =
  Cmd.ofSub stopWs

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
  match msg with
  | OrganizersLoaded (Ok organizers) ->
      { model with Organizers = Resolved organizers }
      |> withoutCmds

  | OrganizersLoaded (Result.Error _) ->
      model |> withoutCmds

  | ConferencesLoaded (Ok conferences) ->
      { model with Conferences = Resolved conferences }
      |> withoutCmds

  | ConferencesLoaded (Result.Error _) ->
      model |> withoutCmds

  | ConferenceLoaded (Ok conference) ->
      model
      |> withView ((VotingPanel,conference,Live) |> Edit)
      |> withoutCmds

  | ConferenceLoaded (Result.Error _) ->
      model |> withoutCmds

  | Received (Connected) ->
      model, Cmd.batch [ Api.queryConferences ; Api.queryOrganizers ]

  | Received (Events events) ->
      match model.View with
      | Edit (editor, conference, Live) ->
          let newConference =
            events
            |> List.filter (eventIsForConference conference.Id)
            |> List.map (fun envelope -> envelope.Event)
            |> Api.Local.evolve conference

          model
          |> withView ((editor,newConference,Live) |> Edit)
          |> withReceivedEvents events

      | _ ->
          model |> withoutCmds

  | WhatIfMsg msg ->
      match model.View with
      | Edit (_, conference, Live) ->
          model
          |> withApiCommand (Api.commandEnvelopeForMsg conference.Id msg)

      | Edit (editor, conference, WhatIf whatif) ->
          let whatIfView =
            updateWhatIfView
              editor
              conference
              whatif
              (Api.commandEnvelopeForMsg conference.Id msg)
              (Api.Local.behaviourFor msg)

          model
          |> withView whatIfView
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
          |> withCmd (Cmd.batch [cmds ; Api.queryConference conference.Id])

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
          conference.Id |> Api.queryConference

      | _ ->
          model |> withoutCmds

  | SwitchToConference conferenceId ->
      model, conferenceId |> Api.queryConference

  | SwitchToEditor target ->
      match model.View with
      | Edit (_, conference, mode) ->
          let editor =
            match target with
            | AvailableEditor.ConferenceInformation ->
                ConferenceInformation.State.init conference.Title (conference.AvailableSlotsForTalks |> string)
                |> ConferenceInformation

            | AvailableEditor.VotingPanel ->
                VotingPanel

            | AvailableEditor.Organizers ->
                Organizers

          model
          |> withView ((editor, conference, mode) |> Edit)
          |> withoutCmds

      | _ ->
          model |> withoutCmds

  | SwitchToNewConference ->
      model
      |> withView (ConferenceInformation.State.init "" "" |> ScheduleNewConference)
      |> withoutCmds

  | ResetConferenceInformation ->
      match model.View with
      | Edit (ConferenceInformation _, conference, mode) ->
          let editor =
            ConferenceInformation.State.init conference.Title (conference.AvailableSlotsForTalks |> string)
            |> ConferenceInformation

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
          |> withCmd (Cmd.batch [ titleCmd ; availableSlotsForTalksCmd ])

      | _ ->
          model |> withoutCmds

  | Msg.ScheduleNewConference ->
      match model.View with
      | ScheduleNewConference submodel when submodel |> ConferenceInformation.Types.isValid ->
          let title =
            ConferenceInformation.Types.title submodel

          let availableSlotsForTalks =
            ConferenceInformation.Types.availableSlotsForTalks submodel

          let conference =
            emptyConference()
            |> withTitle title
            |> withAvailableSlotsForTalks availableSlotsForTalks

          let editor =
            ConferenceInformation.State.init conference.Title (conference.AvailableSlotsForTalks |> string)
            |> ConferenceInformation

          model
          |> withView ((editor, conference, Live) |> Edit)
          |> withApiCommand (API.Command.conferenceApi.ScheduleConference conference conference.Id)

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

  | CommandResponse (transaction, Ok eventEnvelopes) ->
      let notifications =
        eventEnvelopes
        |> List.map Notifications.fromEventEnvelopes
        |> Cmd.batch

      model
      |> withoutTransaction transaction
      |> withCmd notifications

    | CommandResponse (transaction, Result.Error error) ->
        model |> withoutCmds
        // TODO: damit umgehen

