module CommandApiHandler

open System.IO
open Suave
open Suave.Logging
open System.Net
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open System
open Suave.ServerErrors
open Suave.Logging
open Suave.Logging.Message

open Server.Auth
open Server.FableJson

let logger = Log.create "Commands Api"

/// Handle the POST on /api/commands
let handlePost (ctx: HttpContext) =
    useToken ctx (fun token -> async {


      let command =
          ctx.request.rawForm
          |> System.Text.Encoding.UTF8.GetString
          |> ofJson<Commands.Command>

      logger.info (eventX "Handle Command: {command}" >> setField "command" command)

      // wenn token.id nicht id im command entspricht: return! UNAUTHORIZED
      return! Successful.OK (toJson command) ctx
    })
