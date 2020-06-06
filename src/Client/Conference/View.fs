module Conference.View

open Conference.Types

open EventSourced
open Fable.React
open Fable.React.Props
open Fable.FontAwesome

open Utils.Elmish
open Domain
open Domain.Events
open Domain.Model

open Fulma
open Fulma.Extensions.Wikiki

let private pleaseSelectAConference =
  "Please select a conference"

let private renderMessageType messageType =
  match messageType with
  | NotificationType.Info -> "is-info"
  | NotificationType.Success -> "is-success"
  | NotificationType.Warning -> "is-danger"

let private messageWindow name content messageType =
  let mapper =
     sprintf "%A" >> str >> List.singleton >> li []

  article [ ClassName <| "message " + renderMessageType messageType ]
    [
      div  [ ClassName "message-header" ]
        [ name |> str ]
      div [ ClassName "message-body" ]
        [
          ul []
            [
              yield! content |> List.map mapper
            ]
        ]
    ]

let private messageWindowType events =
  match events |> List.exists (function | DomainError _ -> true | _ -> false) with
  | true -> NotificationType.Warning
  | false -> NotificationType.Success

let private renderSpeakers (speakers : Speaker list) =
  speakers
  |> List.map (fun s -> s.Firstname + " " + s.Lastname)
  |> String.concat ", "

let private viewVotingButton voteMsg revokeVotingMsg isActive btnType label =
  let clickMsg =
    if isActive
    then revokeVotingMsg
    else voteMsg

  Button.a
    [
      Button.Props [ OnClick clickMsg ]
      if isActive then
        Button.Color btnType
    ]
    [ label |> str ]

let private viewVotingButtons dispatch user vote (talk : Model.ConferenceAbstract) =
  let possibleVotings =
    [
      Voting (talk.Id, user, Two), IsPrimary, "2"
      Voting (talk.Id, user, One), IsPrimary, "1"
      Voting (talk.Id, user, Zero), IsPrimary, "0"
      Voting (talk.Id, user, Veto), IsDanger, "Veto"
    ]

  let buttonMapper (voting,btnType,label) =
    viewVotingButton
      (fun _ -> voting |> Vote |> WhatIfMsg |> dispatch)
      (fun _ -> voting |> RevokeVoting |> WhatIfMsg |> dispatch)
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
  div []
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

let private viewOrganizer dispatch conference (organizer : Organizer) =
  let isAddedToConference =
    conference.Organizers
    |> List.exists (fun o -> organizer.Id = o.Id)

  let changeMsg =
    if isAddedToConference
    then RemoveOrganizerFromConference
    else AddOrganizerToConference

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
      Columns.columns []
        [
          Column.column [] [ Heading.h1 [ Heading.Is4 ]  [ str "Name" ] ]
          Column.column [] [ Heading.h1 [ Heading.Is4 ]  [ str "Added" ] ]
        ]

      yield! organizers |> List.map (viewOrganizer dispatch conference)
    ]

let private viewOrganizersPanel dispatch conference organizers =
  match organizers with
  | Resolved organizers ->
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

let footer (model : Model) =
  let content =
    match model.View with
    | Edit (_,_,mode) ->
        let window =
          match mode with
          | WhatIf whatIf ->
              let commands =
                whatIf.Commands |> List.map (fun commandEnvelope -> commandEnvelope.Command)

              div []
                [
                  messageWindow "Potential Commands" commands NotificationType.Info
                  messageWindow "Potential Events" whatIf.Events (messageWindowType whatIf.Events)
                ]

          | Live ->
              let events =
                model.LastEvents |> List.map (fun ee -> ee.Event)

              let commands =
                model.OpenTransactions |> Map.toList

              div []
                [
                  messageWindow "Running Commands" commands NotificationType.Info
                  messageWindow "Last Events" events (messageWindowType events)
                ]

        div [] [ window ]

    | _ ->
      str "No conference loaded"

  content
  |> List.singleton
  |> Container.container [ Container.IsFluid ]

let viewConferenceDropdownItem dispatch (conferenceId, title) =
  Dropdown.Item.a
    [ Dropdown.Item.Props [ OnClick (fun _ -> conferenceId |> SwitchToConference |> dispatch) ] ]
    [ str title ]

let private viewActiveConference currentView =
  match currentView with
  | Edit (_, conference, _) ->
      conference.Title

  |  NotAsked ->
      pleaseSelectAConference

  | ScheduleNewConference _ ->
      "New Conference"

  | Loading ->
      "Loading..."

  | NotSuccessful _ ->
      "Error loading conference"

let private viewConferenceList dispatch currentView conferences =
  match conferences with
  | Resolved conferences ->
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

let private viewAllOrNothing dispatch =
  Button.a
    [
      Button.Color IsPrimary
      Button.Props [ OnClick (fun _ -> AllOrNothing |> dispatch) ]
    ]
    [
      Icon.icon [ Icon.Size IsSmall ] [ Fa.i [ Fa.Solid.CheckSquare ] [] ]
      span [] [ "Make it so (all)" |> str ]
    ]

let private viewModeControls dispatch currentView =
  match currentView with
  | Edit (_,_,WhatIf _) ->
      [
        viewMakeItSo dispatch
        viewAllOrNothing dispatch
        viewWhatIfSwitch dispatch true
      ]

  | Edit (_,_,Live) ->
      [ viewWhatIfSwitch dispatch false ]

  | _ ->
      []

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
  Container.container [ Container.IsFluid ]
    [
      viewHeaderLine dispatch currentView conferences
      viewTabs currentView (SwitchToEditor>>dispatch)
    ]


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
    "Edit"

let viewCurrentView dispatch user currentView organizers =
  Container.container [ Container.IsFluid ]
    [
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
          div [ ClassName "columns" ]
            [
              div [ ClassName "column"] [ "Conference Not Loaded" |> str ]
            ]
    ]

let view dispatch model =
  div []
    [
      Section.section [] [ viewHeader dispatch model.View model.Conferences ]
      Section.section [] [ viewCurrentView dispatch model.Organizer model.View model.Organizers ]
      Footer.footer [] [ footer model ]
    ]
