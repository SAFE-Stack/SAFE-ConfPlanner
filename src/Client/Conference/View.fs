module Conference.View

open Conference.Types

open Fable.Helpers.React
open Fable.Helpers.React.Props

open Global
open Model
open Events

open Fulma.Layouts
open Fulma.Components
open Fulma.Elements
open Fulma.Extra.FontAwesome
open Fulma.Elements.Form


type MessageType =
  | Info
  | Success
  | Error

let pleaseSelectAConference =
  "Please select a conference"

let renderMessageType messageType =
  match messageType with
  | Info -> "is-info"
  | Success -> "is-success"
  | Error -> "is-danger"

let speaker (speakers : Speaker list) =
  speakers
  |> List.map (fun s -> s.Firstname + " " + s.Lastname)
  |> String.concat ", "


let viewVotingButtons dispatch user vote (talk : Model.ConferenceAbstract) =
  Field.field_div
    [
      Field.hasAddons
    ]
    [
      Control.control_div []
        [
          Button.button_a
            [
              if vote = Some (Vote (talk.Id, user, Two)) then
                yield Button.isActive
            ]
            [ str "2" ]
        ]
      Control.control_div []
        [
          Button.button_a
            [
              if vote = Some (Vote (talk.Id, user, One)) then
                yield Button.isActive
            ]
            [ str "1" ]
        ]
      Control.control_div []
        [
          Button.button_a
            [
              if vote = Some (Vote (talk.Id, user, Zero)) then
                yield Button.isActive
            ]
            [ str "0" ]
        ]

      Control.control_div []
        [
          Button.button_a
            [
              if vote = Some (Veto (talk.Id, user)) then
                yield Button.isActive
            ]
            [ str "Veto" ]
        ]
    ]


let viewTalk dispatch user votings (talk : Model.ConferenceAbstract) =
  let vote =
    votings |> extractVoteForAbstract user talk.Id


  Card.card [ Card.props [Style [ MarginTop 5; MarginBottom 5 ]] ]
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
                      p [ ClassName "subtitle is-6" ] [ str <| speaker talk.Speakers ]
                    ]
                ]
              Content.content []
                [str "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus nec iaculis mauris."]
            ]
        ]
      Card.footer [ ]
        [ Card.Footer.item [ ]
            [ viewVotingButtons dispatch user vote talk ]
        ]
    ]

let simpleButton txt action dispatch =
  Column.column []
    [
       a
        [
          ClassName "button"
          OnClick (fun _ -> action |> dispatch)
        ]
        [ str txt ]
    ]


let abstractColumn dispatch color filter user conference  =
  let abstracts =
    conference.Abstracts
    |> List.filter filter
    |> List.map (viewTalk dispatch user conference.Votings)

  let style : ICSSProp list =
    [
      BackgroundColor color
      Display Flex
      FlexDirection "column"
    ]

  abstracts
  |> Column.column [ Column.props [ Style style ] ]

let proposedColumn dispatch user conference =
  conference |> abstractColumn dispatch "#dddddd" (fun abs -> abs.Status = Proposed) user

let acceptedColumn dispatch user conference =
  conference |> abstractColumn dispatch "#ddffdd" (fun abs -> abs.Status = Accepted) user

let rejectedColumn dispatch user conference =
  conference |> abstractColumn dispatch "#ffdddd" (fun abs -> abs.Status = Rejected) user

let toggleModeButtons dispatch mode =
  let buttons =
    match mode with
    | Live ->
        [ simpleButton "Switch to WhatIf-Mode" ToggleMode dispatch ]

    | WhatIf _ ->
        [
          simpleButton "Switch to Live-Mode" ToggleMode dispatch
          simpleButton "Make It So" MakeItSo dispatch
        ]

  buttons |> Columns.columns []

let private viewVotingPanelHeader header =
  [ header |> str ]
  |> Heading.h1 [ Heading.is3 ]
  |> List.singleton
  |> Column.column []

let private viewVotingPanel dispatch user conference =
  [
    [ "Proposed"; "Accepted" ; "Rejected"]
    |> List.map viewVotingPanelHeader
    |> Columns.columns []

    Columns.columns []
      [
        conference |> proposedColumn dispatch user
        conference |> acceptedColumn dispatch user
        conference |> rejectedColumn dispatch user
      ]

    Columns.columns []
      [
        match conference.VotingPeriod with
        | InProgess ->
            yield simpleButton "Finish Votingperiod" FinishVotingperiod dispatch

        | Finished ->
            yield simpleButton "Reopen Votingperiod" ReopenVotingperiod dispatch
      ]
  ]
  |> div []

let messageWindow name content messageType =
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

let messageWindowType events =
  match events |> List.exists isError with
  | true -> MessageType.Error
  | false -> MessageType.Success

let footer dispatch conference lastEvents =
  let content =
    match conference with
    | RemoteData.Success (conference,mode) ->
        let window =
          match mode with
          | WhatIf whatif ->
              let commands =
                whatif.Commands |> List.map (fun (_,commands) -> commands)

              [
                messageWindow "Potential Commands" commands MessageType.Info
                messageWindow "Potential Events" whatif.Events <| messageWindowType whatif.Events
              ]
              |> div []

          | Live ->
              [
                messageWindow "Last Events" lastEvents <| messageWindowType lastEvents
              ]
              |> div []

        [
          mode |> toggleModeButtons dispatch
          window
        ]
        |> div []


    | _ ->
      str "No conference loaded"

  content
  |> List.singleton
  |> Container.container [ Container.isFluid ]

let viewConferenceDropdownItem dispatch (conferenceId, title) =
  Dropdown.item
    [
      Dropdown.Item.props [ OnClick (fun _ -> conferenceId |> SwitchToConference |> dispatch) ]
    ]
    [ str title ]

let private viewActiveConference conference =
  match conference with
  | RemoteData.NotAsked ->
      pleaseSelectAConference

  | RemoteData.Success (conference,_) ->
      conference.Title

  | RemoteData.Loading ->
      "Loading..."

  | RemoteData.Failure _ ->
      "Error loading conference"

let viewConferences dispatch conference conferences =
  match conferences with
  | RemoteData.Success conferences ->
      [
        div []
          [
            Button.button_a []
              [
                span [] [ conference |> viewActiveConference |> str ]
                Icon.faIcon [ Icon.isSmall ] [ Fa.icon Fa.I.AngleDown ]
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
      |> Dropdown.dropdown [ Dropdown.isHoverable ]

  | _ ->
      [ div [ ClassName "column"] [ "Conferences not loaded" |> str ] ]
      |> div [ ClassName "columns" ]


let viewHeader dispatch conference conferences =
  [
    Panel.panel []
      [
        Panel.heading [ ] [ str "Conferences"]
        Panel.block [ ]
          [
            conferences |> viewConferences dispatch conference
          ]
      ]
  ]
  |> Container.container [ Container.isFluid ]

let viewConference dispatch user conference =
  match conference with
  | RemoteData.Success (conference,_) ->
      viewVotingPanel dispatch user conference

  | _ ->
      [
        div [ ClassName "column"] [ "Conference Not Loaded" |> str ]
      ]
      |> div [ ClassName "columns" ]

  |> List.singleton
  |> Container.container [ Container.isFluid ]

let root model dispatch =
  [
    viewHeader dispatch model.Conference model.Conferences |> List.singleton |> Section.section []
    viewConference dispatch model.Organizer model.Conference |> List.singleton |> Section.section []
    footer dispatch model.Conference model.LastEvents |> List.singleton |> Footer.footer []
  ]
  |> div []
