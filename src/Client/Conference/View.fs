module Conference.View

open Conference.Types

open Fable.Helpers.React
open Fable.Helpers.React.Props

open Global
open Model
open Events

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

let viewAbstracts conference mode dispatch =
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

let footer mode lastEvents dispatch =
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

let viewPage conference lastEvents mode dispatch =
  div []
    [
      viewAbstracts conference mode dispatch
      footer mode lastEvents dispatch
    ]

let root (model : Model) dispatch =
  match model.State with
     | RemoteData.Success conference ->
         viewPage conference model.LastEvents model.Mode dispatch

     | _ ->
        div [ ClassName "columns is-vcentered" ]
          [
            div [ ClassName "column"] [ "State Not Loaded" |> str ]
          ]
