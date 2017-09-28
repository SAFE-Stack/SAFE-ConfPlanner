module Conference.State

open Elmish
open Fable.PowerPack
open Fable.PowerPack.Fetch.Fetch_types
open Fable.Core.JsInterop
open Global
open Conference.View


let init (user: UserData option) : Model * Cmd<Msg> =
  match user with
  | Some user ->
      { Token = user.Token },Cmd.none

  | None ->
      { Token = "" },Cmd.none


let postCommand (token,command: Commands.Command) =
  promise {
    let url = "api/commands"
    let body = toJson command
    let props =
      [
        RequestProperties.Method HttpMethod.POST
        Fetch.requestHeaders
          [
            HttpRequestHeaders.Authorization ("Bearer " + token)
            HttpRequestHeaders.ContentType "application/json"
          ]
        RequestProperties.Body !^body
      ]

    try
      let! response = Fetch.fetch url props

      if not response.Ok then
        return! failwithf "Error: %d" response.Status
      else
        let! data = response.text()
        return data
    with
    | _ -> return! failwithf "Could not post command."
  }

let postCommandCmd data =
  Cmd.ofPromise postCommand data PostCommandSuccess PostCommandError

let update msg model =
  match msg with
  | FinishVotingPeriod ->
      model, postCommandCmd (model.Token,Commands.Command.FinishVotingPeriod)

  | PostCommandSuccess status -> model, Cmd.none

  | PostCommandError(_) -> model, Cmd.none
