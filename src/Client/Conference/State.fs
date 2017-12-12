module Conference.State

open Elmish
open Global

open Server.ServerTypes
open Infrastructure.Types

open Conference.Types
open Conference.Ws
open Conference.Api
open Fable.Import.Browser
open Domain
open Domain.Model

let private updateStateWithEvents conference events  =
  events |> List.fold Domain.Projections.apply conference

let private makeStreamId (Model.ConferenceId id) =
  id |> string |> StreamId

let private makeConferenceId (StreamId id) =
  id |> System.Guid.Parse |> Model.ConferenceId

let private eventSetIsForCurrentConference ((_,streamId),_) conference =
  streamId |> makeConferenceId = conference.Id

let private commandHeader transaction id =
  transaction, id |> makeStreamId

let private queryConference conferenceId =
  conferenceId
  |> API.QueryParameter.Conference
  |> createQuery
  |> ClientMsg.Query
  |> wsCmd

let private queryConferences =
  API.QueryParameter.Conferences
  |> createQuery
  |> ClientMsg.Query
  |> wsCmd

let private queryOrganizers =
  API.QueryParameter.Organizers
  |> createQuery
  |> ClientMsg.Query
  |> wsCmd

let init() =
  {
    View = CurrentView.NotAsked
    Conferences = RemoteData.NotAsked
    Organizers = RemoteData.NotAsked
    LastEvents = []
    Organizer = OrganizerId <| System.Guid.Parse "311b9fbd-98a2-401e-b9e9-bab15897dad4"
    OpenTransactions = []
    OpenNotifications = []
  }, Cmd.ofSub startWs

let private timeoutCmd msg dispatch =
  window.setTimeout((fun _ -> msg |> dispatch), 6000) |> ignore

let private withView view model =
  { model with View = view }

let private withOpenTransaction transaction model =
   { model with OpenTransactions = transaction :: model.OpenTransactions}

let private withOpenTransactions transactions model =
  { model with OpenTransactions = List.concat [transactions ; model.OpenTransactions ] }

let private withLastEvents events model =
  { model with LastEvents = events }

let private withCommands commands model =
  model, commands

let private withoutCommands model =
  model, Cmd.none

let withFinishedTransaction transaction events model =
  if model.OpenTransactions |> List.exists (fun openTransaction -> transaction = openTransaction) then
    let model =
      { model with
          OpenTransactions =  model.OpenTransactions |> List.filter (fun openTransaction -> transaction <> openTransaction)
          OpenNotifications = model.OpenNotifications @ events
      }

    let commands =
      events
      |> List.map (RemoveNotification>>timeoutCmd>>Cmd.ofSub)
      |> Cmd.batch

    model |> withCommands commands
  else
    model |> withoutCommands


let withoutNotification notification model =
  { model with OpenNotifications = model.OpenNotifications |> List.filter (fun event -> event <> notification) }


let private updateWhatIfView editor conference whatif behaviour command =
  let events =
      conference |> behaviour

  let newConference =
    events |> updateStateWithEvents conference

  let transaction =
    transactionId()

  let commands =
     (conference.Id |> commandHeader transaction, command) :: whatif.Commands

  let whatif =
    WhatIf <|
      {
        whatif with
          Events = events
          Commands = commands
      }

  (editor, newConference, whatif) |> Edit

let private updateWhatIf msg editor conference whatif =
  let updateWhatIfView =
    updateWhatIfView
      editor
      conference
      whatif

  match msg with
  | Vote voting ->
      updateWhatIfView
        (voting |> Behaviour.vote)
        (voting |> Commands.Vote)

  | RevokeVoting voting ->
      updateWhatIfView
        (voting |> Behaviour.vote)
        (voting |> Commands.Vote)

  | FinishVotingperiod ->
      updateWhatIfView
        Behaviour.finishVotingPeriod
        Commands.FinishVotingPeriod

  | ReopenVotingperiod ->
      updateWhatIfView
        Behaviour.reopenVotingPeriod
        Commands.ReopenVotingPeriod

  | AddOrganizerToConference organizer ->
      updateWhatIfView
        (organizer |> Behaviour.addOrganizerToConference)
        (organizer |> Commands.AddOrganizerToConference)

  | RemoveOrganizerFromConference organizer ->
      updateWhatIfView
        (organizer |> Behaviour.removeOrganizerFromConference)
        (organizer |> Commands.RemoveOrganizerFromConference)

  | ChangeTitle title ->
      updateWhatIfView
        (title |> Behaviour.changeTitle)
        (title |> Commands.ChangeTitle)


  | DecideNumberOfSlots number ->
      updateWhatIfView
        (number |> Behaviour.decideNumberOfSlots)
        (number |> Commands.DecideNumberOfSlots)

let private liveUpdateCommand msg =
  match msg with
  | Vote voting ->
      voting |> Commands.Vote

  | RevokeVoting voting ->
      voting |> Commands.RevokeVoting

  | FinishVotingperiod ->
     Commands.FinishVotingPeriod

  | ReopenVotingperiod ->
     Commands.ReopenVotingPeriod

  | AddOrganizerToConference organizer ->
      organizer |> Commands.AddOrganizerToConference

  | RemoveOrganizerFromConference organizer ->
      organizer |> Commands.RemoveOrganizerFromConference

  | ChangeTitle title ->
      title |> Commands.ChangeTitle

  | DecideNumberOfSlots number ->
      number |> Commands.DecideNumberOfSlots

let private withWsCmd command conference model =
  let transaction =
    transactionId()

  model
  |> withOpenTransaction transaction
  |> withCommands (wsCmd <| ClientMsg.Command (conference.Id |> commandHeader transaction, command))

let withLiveUpdateCmd conference whatifMsg model =
  let transaction =
    transactionId()

  model
  |> withOpenTransaction transaction
  |> withCommands (wsCmd <| ClientMsg.Command (conference.Id |> commandHeader transaction, liveUpdateCommand whatifMsg))

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
  match msg with
  | Received (ServerMsg.QueryResponse response) ->
      match response.Result with
      | NotHandled ->
          model |> withoutCommands

      | Handled result ->
          match result with
          | API.QueryResult.Conference conference ->
              model
              |> withView ((VotingPanel,conference,Live) |> Edit)
              |> withoutCommands

          | API.QueryResult.Conferences conferences ->
              { model with Conferences = conferences |> RemoteData.Success }
              |> withoutCommands

          | API.QueryResult.Organizers organizers ->
              { model with Organizers = organizers |> RemoteData.Success }
              |> withoutCommands

          | API.QueryResult.ConferenceNotFound ->
              model |> withoutCommands

  | Received (ServerMsg.Connected) ->
      model, Cmd.batch [ queryConferences ; queryOrganizers ]

  | Received (ServerMsg.Events eventSet) ->
      match model.View with
      | Edit (editor, conference, Live) when eventSetIsForCurrentConference eventSet conference  ->
          let transaction,events =
            eventSet |> (fun ((transaction,_),events) -> transaction,events)

          let newConference =
            events |> updateStateWithEvents conference

          model
          |> withView ((editor,newConference,Live) |> Edit)
          |> withLastEvents events
          |> withFinishedTransaction transaction events

      | _ ->
          model |> withoutCommands

  | WhatIfMsg whatifMsg ->
      match model.View with
      | Edit (_, conference, Live) ->
          model
          |> withLiveUpdateCmd conference whatifMsg

      | Edit (editor, conference, WhatIf whatif) ->
          model
          |> withView (updateWhatIf whatifMsg editor conference whatif)
          |> withoutCommands

      | _ ->
           model |> withoutCommands

  | MakeItSo ->
      match model.View with
      | Edit (editor, conference, WhatIf whatif)  ->
          let commands =
            whatif.Commands
            |> List.rev
            |> List.collect (ClientMsg.Command >> wsCmd)

          model
          |> withView ((editor,whatif.Conference,Live) |> Edit)
          |> withOpenTransactions (whatif.Commands |> List.map messageAsTransactionId)
          |> withCommands (Cmd.batch [commands ; conference.Id |> queryConference])

      | _ ->
          model |> withoutCommands

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
          |> withoutCommands

      | Edit (editor, conference, WhatIf _) ->
          { model with View = (editor, conference, Live) |> Edit },
          conference.Id |> queryConference

      | _ ->
          model |> withoutCommands

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
          |> withoutCommands

      | _ ->
          model |> withoutCommands

  | SwitchToNewConference ->
      model
      |> withView (ConferenceInformation.State.init "" "" |> CurrentView.ScheduleNewConference)
      |> withoutCommands

  | ResetConferenceInformation ->
      match model.View with
      | Edit (ConferenceInformation _, conference, mode) ->
          let editor =
            ConferenceInformation.State.init conference.Title (conference.AvailableSlotsForTalks |> string)
            |> Editor.ConferenceInformation

          model
          |> withView ((editor, conference, mode) |> Edit)
          |> withoutCommands

      | _ ->
          model |> withoutCommands

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
          |> withCommands (Cmd.batch [ titleCmd ; availableSlotsForTalksCmd ])

      | _ ->
          model |> withoutCommands

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
          model |> withoutCommands

  | ConferenceInformationMsg msg ->
      match model.View with
      | Edit (ConferenceInformation submodel, conference, mode) ->
          let newSubmodel =
            submodel |> ConferenceInformation.State.update msg

          model
          |> withView ((ConferenceInformation newSubmodel, conference, mode) |> Edit)
          |> withoutCommands

      | ScheduleNewConference submodel ->
          let view =
            submodel
            |> ConferenceInformation.State.update msg
            |> ScheduleNewConference

          model
          |> withView view
          |> withoutCommands

      | _ ->
          model |> withoutCommands

  | RemoveNotification notification ->
      model
      |> withoutNotification notification
      |> withoutCommands




