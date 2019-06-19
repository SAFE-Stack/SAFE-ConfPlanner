module Conference.View

open Conference.Types

open Fable.React
open Fable.React.Props
open Fable.FontAwesome

open Global
open Domain
open Domain.Model

open Fulma
open Fulma.Extensions.Wikiki

let private pleaseSelectAConference =
  "Please select a conference"

let private renderMessageType messageType =
  match messageType with
  | NotificationType.Info -> "is-info"
  | NotificationType.Success -> "is-success"
  | NotificationType.Error -> "is-danger"

let private messageWindow name content messageType =
  let mapper =
     sprintf "%A" >> str >> List.singleton >> li []

  article [ ClassName <| "message " + renderMessageType messageType ]
    [
      div  [ ClassName "message-header" ]
        [
          name |> str
        ]
      div [ ClassName "message-body" ]
        [
          ul []
            [
              yield! content |> List.map mapper
            ]
        ]
    ]

let private messageWindowType events =
  match events |> List.exists (function | Domain.Events.Error _ -> true | _ -> false) with
  | true -> NotificationType.Error
  | false -> NotificationType.Success

let private renderSpeakers (speakers : Speaker list) =
  speakers
  |> List.map (fun s -> s.Firstname + " " + s.Lastname)
  |> String.concat ", "

let private viewVotingButton voteMsg revokeVotingMsg isActive btnType label =
  let clickMsg =
    if isActive then
      revokeVotingMsg
    else
      voteMsg

  Button.a
    [
      yield Button.Props [ OnClick clickMsg ]
      if isActive then
        yield Button.Color btnType
    ]
    [ label |> str ]

let private viewVotingButtons dispatch user vote (talk : Domain.Model.ConferenceAbstract) =
  let possibleVotings =
    [
      Voting.Voting (talk.Id, user, Two), IsPrimary, "2"
      Voting.Voting (talk.Id, user, One), IsPrimary, "1"
      Voting.Voting (talk.Id, user, Zero), IsPrimary, "0"
      Voting.Voting (talk.Id, user, Veto), IsDanger, "Veto"
    ]

  let buttonMapper (voting,btnType,label) =
    viewVotingButton
      (fun _ -> voting |> WhatIfMsg.Vote |> WhatIfMsg |> dispatch)
      (fun _ -> voting |> WhatIfMsg.RevokeVoting |> WhatIfMsg |> dispatch)
      (vote = Some voting)
      btnType
      label
    |> List.singleton
    |> Control.div []

  Field.div
    [ Field.HasAddons  ]
    [
      yield! possibleVotings |> List.map buttonMapper
    ]

let private viewTalk dispatch user votings (talk : Model.ConferenceAbstract) =
  let vote =
    votings |> extractVoteForAbstract user talk.Id

  let cardStyle : CSSProp list =
    [
      MarginTop 5
      MarginBottom 5
    ]

  Card.card [ Props [ Style cardStyle ] ]
    [
      Card.header []
        [
          Card.Header.title [] [ str <| talk.Text ]
          // Card.Header.icon [] [ i [ ClassName "fa fa-angle-down" ] [ ] ]
        ]
      Card.content []
        [
          Content.content []
            [
              Media.media []
                [
                  Media.content []
                    [
                      p [ ClassName "subtitle is-6" ] [ str <| renderSpeakers talk.Speakers ]
                    ]
                ]
              Content.content []
                [ str "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus nec iaculis mauris."]
              Content.content []
                [  viewVotingButtons dispatch user vote talk ]
            ]
        ]
    ]

let private simpleButton txt dispatch action =
  Column.column []
    [
      Button.a
        [ Button.OnClick (fun _ -> action |> dispatch) ]
        [
          span [] [ str txt ]
        ]
    ]

let private abstractColumn dispatch color filter user conference  =
  let abstracts =
    conference.Abstracts
    |> List.filter filter
    |> List.map (viewTalk dispatch user conference.Votings)

  let style : CSSProp list =
    [
      BackgroundColor color
      Display DisplayOptions.Flex
      FlexDirection "column"
    ]

  abstracts
  |> Column.column [ Column.Props [ Style style ] ]

let private proposedColumn dispatch user conference =
  conference |> abstractColumn dispatch "#dddddd" (fun abs -> abs.Status = Proposed) user

let private acceptedColumn dispatch user conference =
  conference |> abstractColumn dispatch "#ddffdd" (fun abs -> abs.Status = Accepted) user

let private rejectedColumn dispatch user conference =
  conference |> abstractColumn dispatch "#ffdddd" (fun abs -> abs.Status = Rejected) user

let private viewVotingPanelHeader header =
  [ header |> str ]
  |> Heading.h1 [ Heading.Is3 ]
  |> List.singleton
  |> Column.column []

let private viewVotingPanel dispatch user conference =
  [
    Columns.columns []
      [
        match conference.VotingPeriod with
        | InProgress ->
            yield simpleButton "Finish Votingperiod" (WhatIfMsg>>dispatch) FinishVotingperiod
        | Finished ->
            yield simpleButton "Reopen Votingperiod" (WhatIfMsg>>dispatch) ReopenVotingperiod
      ]

    [ "Proposed"; "Accepted" ; "Rejected"]
    |> List.map viewVotingPanelHeader
    |> Columns.columns []

    Columns.columns []
      [
        conference |> proposedColumn dispatch user
        conference |> acceptedColumn dispatch user
        conference |> rejectedColumn dispatch user
      ]
  ]
  |> div []

let private viewOrganizer dispatch conference (organizer : Organizer) =
  let isAddedToConference =
    conference.Organizers
    |> List.exists (fun o -> organizer.Id = o.Id)

  let changeMsg =
    if isAddedToConference then
      RemoveOrganizerFromConference
    else
      AddOrganizerToConference

  let switch =
    Switch.switch
      [
        Switch.Id ("switch-organizer" + (string organizer.Id))
        Switch.Checked isAddedToConference
        Switch.IsRounded
        Switch.Color IsPrimary
        Switch.OnChange (fun _ -> organizer |> changeMsg |> WhatIfMsg |> dispatch)
      ] []
  [
    Column.column [] [ str <| organizer.Firstname + " " + organizer.Lastname ]
    Column.column [] [ switch ]
  ]
  |> Columns.columns []

let private viewOrganizers dispatch conference organizers =
  div []
    [
      yield Columns.columns []
        [
          Column.column [] [ Heading.h1 [ Heading.Is4 ]  [ str "Name" ] ]
          Column.column [] [ Heading.h1 [ Heading.Is4 ]  [ str "Added" ] ]
        ]

      yield! organizers |> List.map (viewOrganizer dispatch conference)
    ]

let private viewOrganizersPanel dispatch conference organizers =
  match organizers with
  | RemoteData.Success organizers ->
      Columns.columns []
        [
          Column.column
            [
              Column.Width (Screen.All, Column.IsHalf)
              Column.Offset (Screen.All, Column.IsOneThird)
            ]
            [
              viewOrganizers dispatch conference organizers
            ]
        ]

  | _ ->
      div [] [ "no organizers" |> str ]

let footer currentView lastEvents =
  let content =
    match currentView with
    | Edit (_,_,mode) ->
        let window =
          match mode with
          | WhatIf whatif ->
              let commands =
                whatif.Commands |> List.map (fun (_,commands) -> commands)

              [
                messageWindow "Potential Commands" commands NotificationType.Info
                messageWindow "Potential Events" whatif.Events <| messageWindowType whatif.Events
              ]
              |> div []

          | Live ->
              [
                messageWindow "Last Events" lastEvents <| messageWindowType lastEvents
              ]
              |> div []

        [
          window
        ]
        |> div []


    | _ ->
      str "No conference loaded"

  content
  |> List.singleton
  |> Container.container [ Container.IsFluid ]

let viewConferenceDropdownItem dispatch (conferenceId, title) =
  Dropdown.Item.a
    [
      Dropdown.Item.Props [ OnClick (fun _ -> conferenceId |> SwitchToConference |> dispatch) ]
    ]
    [ str title ]

let private viewActiveConference currentView =
  match currentView with
  | CurrentView.Edit (_, conference, _) ->
      conference.Title

  |  CurrentView.NotAsked ->
      pleaseSelectAConference

  | CurrentView.ScheduleNewConference _ ->
      "New Conference"

  | CurrentView.Loading ->
      "Loading..."

  | CurrentView.Error _ ->
      "Error loading conference"

let private viewConferenceList dispatch currentView conferences =
  match conferences with
  | RemoteData.Success conferences ->
      [
        div []
          [
            Button.a []
              [
                span [] [ currentView |> viewActiveConference |> str ]
                Icon.icon [ Icon.Size IsSmall ] [ Fa.i [ Fa.Solid.AngleDown ] [] ]
              ]
          ]
        Dropdown.menu []
          [
            Dropdown.content []
              [
                yield! conferences |> List.map (viewConferenceDropdownItem dispatch)
              ]
          ]
      ]
      |> Dropdown.dropdown [ Dropdown.IsHoverable ]

  | _ ->
      [ div [ ClassName "column"] [ "Conferences not loaded" |> str ] ]
      |> div [ ClassName "columns" ]


let private viewTab currentView selectEditorMsg targetEditor label =
  match currentView with
  | Edit (currentEditor, _, _) ->
      let currentTarget =
        currentEditor |> matchEditorWithAvailableEditor

      Tabs.tab
        [
          Tabs.Tab.IsActive (currentTarget = targetEditor)
          Tabs.Tab.Props [ OnClick (fun _ -> targetEditor |> selectEditorMsg) ]
        ]
        [ a [ ] [str label] ]

  | _ -> str ""

let private viewTabs currentView selectEditorMsg =
  Tabs.tabs
    [
      Tabs.IsFullWidth
      Tabs.IsBoxed
    ]
    [
      viewTab currentView selectEditorMsg AvailableEditor.ConferenceInformation "Information"
      viewTab currentView selectEditorMsg AvailableEditor.VotingPanel "Votings"
      viewTab currentView selectEditorMsg AvailableEditor.Organizers "Organizers"
    ]

let private viewWhatIfSwitch dispatch isChecked =
  Switch.switch
    [
      Switch.Id "switch-whatif"
      Switch.Checked isChecked
      Switch.IsRounded
      Switch.Color IsPrimary
      Switch.OnChange (fun _ -> ToggleMode |> dispatch)
    ]
    [ str "Whatif" ]

let private viewMakeItSo dispatch =
  Button.a
    [
      Button.Color IsPrimary
      Button.Props [ OnClick (fun _ -> MakeItSo |> dispatch) ]
    ]
    [

      Icon.icon [ Icon.Size IsSmall ] [ Fa.i [ Fa.Solid.CheckSquare ] [] ]
      span [] [ "Make It So" |> str ]
    ]

let private viewModeControls dispatch currentView =
  match currentView with
  | Edit (_,_,WhatIf _) ->
      [
        viewMakeItSo dispatch
        viewWhatIfSwitch dispatch true
      ]

  | Edit (_,_,Live) ->
      [
        viewWhatIfSwitch dispatch false
      ]

  | _ ->
      []

let private notificationType notification =
  match notification with
  | Events.Error _ ->
      Notification.Color IsDanger

  | _ ->
      Notification.Color IsSuccess

let private viewNotification dispatch (notification,transaction,animation) =
  let closeMsg _ =
    (notification,transaction,animation) |> RequestNotificationForRemoval |> dispatch

  let itemStyle : CSSProp list =
    [
      MaxHeight "100px"
      Margin "1em 1em 0 1em"
      Transition "max-height 0.6s, margin-top 0.6s"
    ]

  let leavingItemStyle : CSSProp list =
    itemStyle @
      [
        MaxHeight 0
        MarginTop 0
      ]

  Notification.notification
    [
      match animation with
      | Entered ->
          yield Notification.CustomClass "animated bounceInRight"
          yield Notification.Props [ Style itemStyle ]

      | Leaving ->
          yield Notification.CustomClass "animated fadeOutRightBig"
          yield Notification.Props [ Style leavingItemStyle ]

      yield notification |> notificationType

    ]
    [
      Notification.delete
        [ Props [ OnClick closeMsg ] ] []
      notification |> Events.toString |> str
    ]

let private viewNotifications dispatch notifications =
  let containerStyle : CSSProp list =
    [
      Position PositionOptions.Fixed
      Top 60
      Right 0
      Width "100%"
      MaxWidth "300px"
      Padding 0
      Margin 0
      ZIndex 10.
    ]

  notifications |> List.map (viewNotification dispatch)
  |> div [ Style containerStyle ]

let private viewHeaderLine dispatch currentView conferences =
  let modeButtons =
    viewModeControls dispatch currentView

  let conferenceSelect =
    conferences |> viewConferenceList dispatch currentView

  let newConference =
    Button.a
      [
        Button.Color IsPrimary
        Button.OnClick (fun _ -> SwitchToNewConference |> dispatch)
        Button.OnClick (fun _ -> SwitchToNewConference |> dispatch)
      ]
      [
        Icon.icon
          [ Icon.Size IsSmall ]
          [ Fa.i [ Fa.Solid.PlusSquare ] [] ]

        span
          []
          [ "New Conference" |> str ]
      ]

  let levelLeft =
    Level.left [ ]
      [
        Level.item [] [ conferenceSelect ]
        Level.item [] [ newConference ]
      ]

  let levelRight =
    Level.right []
      [
         yield! modeButtons |> List.map (List.singleton >> Level.item [])
      ]

  Level.level []
    [
      levelLeft
      levelRight
    ]

let private viewHeader dispatch currentView conferences =
  [
    yield viewHeaderLine dispatch currentView conferences
    yield viewTabs currentView (SwitchToEditor>>dispatch)
  ]
  |> Container.container [ Container.IsFluid ]

let private viewConferenceInformation dispatch submodel confirmMsg resetMsg confirmLabel =
  let conferenceInformation =
    ConferenceInformation.View.view (ConferenceInformationMsg>>dispatch) submodel

  let confirmButton =
    Button.a
      [
        Button.OnClick (fun _ -> confirmMsg |> dispatch )
        Button.Color IsPrimary
        Button.Disabled (submodel |> ConferenceInformation.Types.isValid |> not)

      ]
      [ str confirmLabel ]

  let resetButton =
    Button.a
      [
        Button.OnClick (fun _ -> resetMsg |> dispatch)
        Button.Color IsWarning
      ]
      [ str "Reset" ]

  [
    Columns.columns []
      [
        Column.column
          [
            Column.Width (Screen.All, Column.IsHalf)
            Column.Offset (Screen.All, Column.IsOneThird)
          ]
          [
            conferenceInformation
          ]
      ]

    Columns.columns []
      [
        Column.column
          [
            Column.Width (Screen.All, Column.IsHalf)
            Column.Offset (Screen.All, Column.IsOneThird)
          ]
          [
            confirmButton
            resetButton
          ]
      ]
  ]
  |> div []

let private viewNewConferencePanel dispatch submodel =
  viewConferenceInformation
    dispatch
    submodel
    Msg.ScheduleNewConference
    ResetConferenceInformation
    "Create"

let viewConferenceInformationPanel dispatch submodel =
  viewConferenceInformation
    dispatch
    submodel
    UpdateConferenceInformation
    ResetConferenceInformation
    "Save"

let viewCurrentView dispatch user currentView organizers =
  match currentView with
  | Edit (VotingPanel, conference, _) ->
      viewVotingPanel dispatch user conference

  | Edit (Organizers, conference, _) ->
      viewOrganizersPanel dispatch conference organizers

  | Edit (ConferenceInformation submodel, _, _) ->
      viewConferenceInformationPanel dispatch submodel

  | ScheduleNewConference submodel  ->
      viewNewConferencePanel dispatch submodel

  | _ ->
      [
        div [ ClassName "column"] [ "Conference Not Loaded" |> str ]
      ]
      |> div [ ClassName "columns" ]

  |> List.singleton
  |> Container.container [ Container.IsFluid ]

let view dispatch model =
  [
    yield viewNotifications dispatch model.OpenNotifications
    yield viewHeader dispatch model.View model.Conferences |> List.singleton |> Section.section []
    yield viewCurrentView dispatch model.Organizer model.View model.Organizers |> List.singleton |> Section.section []
    yield footer model.View model.LastEvents |> List.singleton |> Footer.footer []
  ]
  |> div []
