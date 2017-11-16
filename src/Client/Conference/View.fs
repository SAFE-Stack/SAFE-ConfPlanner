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

let renderMessageType messageType =
    match messageType with
    | Info -> "is-info"
    | Success -> "is-success"
    | Error -> "is-danger"

let speaker (speakers : Speaker list) =
  speakers
  |> List.map (fun s -> s.Firstname + " " + s.Lastname)
  |> String.concat ", "

let talk (t:Model.ConferenceAbstract) =
  div [ ClassName "card" ]
    [
      div [ ClassName "card-content" ]
        [
          div [ ClassName "media" ]
            [
              div [ ClassName "media-content" ]
                [
                  p [ ClassName "title is-4" ] [ str <| t.Text]
                  p [ ClassName "subtitle is-6" ] [ str <| speaker t.Speakers ]
                ]
            ]

          div [ ClassName "content" ]
            [
              str "Lorem ipsum dolor sit amet, consectetur adipiscing elit.Phasellus nec iaculis mauris."
            ]
        ]
    ]

let simpleButton txt action dispatch =
  div
    [ ClassName "column" ]
    [ a
        [
          ClassName "button"
          OnClick (fun _ -> action |> dispatch)
        ]
        [ str txt ]
    ]


let abstractColumn color filter conference  =
  let abstracts =
    conference.Abstracts
    |> List.filter filter
    |> List.map talk

  div
    [
      ClassName "column"
      Style
        [
          BackgroundColor color
          Display Flex
          FlexDirection "column"
          MinHeight 650
        ]
    ]
    abstracts

let proposedColumn =
  abstractColumn "#dddddd" (fun abs -> abs.Status = Proposed)

let acceptedColumn =
  abstractColumn "#ddffdd" (fun abs -> abs.Status = Accepted)

let rejectedColumn =
  abstractColumn "#ffdddd" (fun abs -> abs.Status = Rejected)

let toggleModeButtons mode dispatch =
  let buttons =
    match mode with
    | Live ->
        [ simpleButton "Switch to WhatIf-Mode" ToggleMode dispatch ]

    | WhatIf _ ->
        [
          simpleButton "Switch to Live-Mode" ToggleMode dispatch
          simpleButton "Make It So" MakeItSo dispatch
        ]

  div [ ClassName "columns" ] buttons

let viewAbstracts dispatch conference =
  div [ ClassName "section"]
    [
      div [ ClassName "columns is-vcentered" ]
        [
          div [ ClassName "column"; Style [ TextAlign "center" ]] [ "Proposed" |> str ]
          div [ ClassName "column"; Style [ TextAlign "center" ]] [ "Accepted" |> str ]
          div [ ClassName "column"; Style [ TextAlign "center" ]] [ "Rejected" |> str ]
        ]

      div [ ClassName "columns is-vcentered" ]
        [
          proposedColumn conference
          acceptedColumn conference
          rejectedColumn conference
        ]

      div [ ClassName "columns is-vcentered" ]
        [
          match conference.VotingPeriod with
          | InProgess ->
              yield simpleButton "Finish Votingperiod" FinishVotingperiod dispatch

          | Finished ->
              yield simpleButton "Reopen Votingperiod" ReopenVotingperiod dispatch
        ]
    ]

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

let footer dispatch mode lastEvents =
  let content =
    match mode with
    | WhatIf whatif ->
        let commands =
          whatif.Commands |> List.map (fun (_,commands) -> commands)

        div []
          [
            messageWindow "Potential Commands" commands MessageType.Info
            messageWindow "Potential Events" whatif.Events <| messageWindowType whatif.Events
          ]

    | Live ->
        div []
          [
            messageWindow "Last Events" lastEvents <| messageWindowType lastEvents
          ]

  footer [ ClassName "footer" ]
    [
      div [ ClassName "container" ]
        [
          div [ ClassName "content" ]
            [
              toggleModeButtons mode dispatch
              content
            ]
        ]
    ]

let viewConferenceDropdownItem dispatch (conferenceId, title) =
  //Dropdown.item [ Dropdown.Item.isActive ] [ str "Item n°3" ]
  Dropdown.item
    [
      Dropdown.Item.props [ OnClick (fun _ -> conferenceId |> SwitchToConference |> dispatch) ]
    ]
    [ str title ]

let viewConferences dispatch conferences =
  match conferences with
  | RemoteData.Success conferences ->
      [
        div []
          [
            Button.button_a []
              [
                span [] [ str "Conferences" ]
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
      |> div [ ClassName "columns is-vcentered" ]

let viewHeader dispatch conferences =
  Section.section []
    [
      Container.container [ Container.isFluid ]
        [
          Panel.panel []
            [
              Panel.heading [ ] [ str "Conferences"]
              Panel.block [ ]
                [
                  conferences |> viewConferences dispatch
                ]
            ]
        ]
    ]

let viewConference dispatch conference =
  match conference with
  | RemoteData.Success conference ->
      viewAbstracts dispatch conference

  | _ ->
      [
        div [ ClassName "column"] [ "Conference Not Loaded" |> str ]
      ]
      |> div [ ClassName "columns is-vcentered" ]

let root model dispatch =
  [
    viewHeader dispatch model.Conferences
    viewConference dispatch model.Conference
    footer dispatch model.Mode model.LastEvents
  ]
  |> div []
