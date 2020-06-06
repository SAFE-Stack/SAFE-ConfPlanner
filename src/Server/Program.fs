/// Server program entry point module.
module Server.Program

open System.IO

let GetEnvVar var =
  match System.Environment.GetEnvironmentVariable(var) with
  | null -> None
  | value -> Some value

let getPortsOrDefault defaultVal =
  match System.Environment.GetEnvironmentVariable("SUAVE_FABLE_PORT") with
  | null -> defaultVal
  | value -> value |> uint16

[<EntryPoint>]
let main args =
  try
    let clientPath =
        match args |> Array.toList with
        | clientPath:: _  when Directory.Exists clientPath ->
            clientPath

        | _ ->
            let clientPath1 = Path.Combine("..","Client")
            let clientPath2 = Path.Combine("src","Client")
            if Directory.Exists clientPath1 then
              clientPath1
            elif Directory.Exists clientPath2 then
              clientPath2
            else
              failwithf "Could not find path to Client directory (tried %s and %s)" clientPath1 clientPath2

    WebServer.start (Path.GetFullPath clientPath) (getPortsOrDefault 8085us)
    0
  with
  | exn ->
      let color = System.Console.ForegroundColor
      System.Console.ForegroundColor <- System.ConsoleColor.Red
      System.Console.WriteLine(exn.Message)
      System.Console.ForegroundColor <- color
      1
