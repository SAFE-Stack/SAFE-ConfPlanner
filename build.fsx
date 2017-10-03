// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO

let clientPath = "./src/Client" |> FullName

let testsPath = "./src/Domain.Tests" |> FullName
let serverPath = "./src/Server" |> FullName


let dotnetcliVersion = "2.0.0"

let mutable dotnetExePath = "dotnet"

// Pattern specifying assemblies to be tested using expecto
// let clientTestExecutables = "test/UITests/**/bin/**/*Tests*.exe"


// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------


let run' timeout cmd args dir =
    if execProcess (fun info ->
        info.FileName <- cmd
        if not (String.IsNullOrWhiteSpace dir) then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) timeout |> not then
        failwithf "Error while running '%s' with args: %s" cmd args

let run = run' System.TimeSpan.MaxValue

let runDotnet workingDir args =
    let result =
        ExecProcess (fun info ->
            info.FileName <- dotnetExePath
            info.WorkingDirectory <- workingDir
            info.Arguments <- args) TimeSpan.MaxValue
    if result <> 0 then failwithf "dotnet %s failed" args

let platformTool tool winTool =
    let tool = if isUnix then tool else winTool
    tool
    |> ProcessHelper.tryFindFileOnPath
    |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"
let npmTool = platformTool "npm" "npm.cmd"
let yarnTool = platformTool "yarn" "yarn.cmd"

do if not isWindows then
    // We have to set the FrameworkPathOverride so that dotnet sdk invocations know
    // where to look for full-framework base class libraries
    let mono = platformTool "mono" "mono"
    let frameworkPath = IO.Path.GetDirectoryName(mono) </> ".." </> "lib" </> "mono" </> "4.5"
    setEnvironVar "FrameworkPathOverride" frameworkPath

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    !!"src/**/bin" ++ "src/**/obj/"
        ++ "test/**/bin" ++ "test/**/obj/"
    |> CleanDirs
    CleanDirs ["bin"; "temp"; "docs/output"; Path.Combine(clientPath,"public/bundle")]
)

Target "InstallDotNetCore" (fun _ ->
    dotnetExePath <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Restore" (fun _ ->
    runDotnet currentDirectory "restore"
)

Target "BuildTests" (fun _ ->
    runDotnet currentDirectory "build"
)


Target "RunTests" (fun _ ->
    runDotnet testsPath "test"
)

Target "InstallClient" (fun _ ->
    printfn "Node version:"
    run nodeTool "--version" __SOURCE_DIRECTORY__
    printfn "Yarn version:"
    run yarnTool "--version" __SOURCE_DIRECTORY__
    run yarnTool "install" __SOURCE_DIRECTORY__
    runDotnet clientPath "restore"
)

Target "BuildClient" (fun _ ->
    runDotnet clientPath "fable yarn-run build"
)

// --------------------------------------------------------------------------------------
// Rename driver for macOS or Linux

// Target "RenameDrivers" (fun _ ->
//     if not isWindows then
//         run npmTool "install phantomjs" ""
//     try
//         if isMacOS && not <| File.Exists "test/UITests/bin/Debug/net461/chromedriver" then
//             Fake.FileHelper.Rename "test/UITests/bin/Debug/net461/chromedriver" "test/UITests/bin/Debug/net461/chromedriver_macOS"
//         elif isLinux && not <| File.Exists "test/UITests/bin/Debug/net461/chromedriver" then
//             Fake.FileHelper.Rename "test/UITests/bin/Debug/net461/chromedriver" "test/UITests/bin/Debug/net461/chromedriver_linux64"
//     with
//     | exn -> failwithf "Could not rename chromedriver at test/UITests/bin/Debug/net461/chromedriver. Message: %s" exn.Message
// )

// Target "RunServerTests" (fun _ ->
//     runDotnet serverTestsPath "run"
// )

// Target "RunClientTests" (fun _ ->
//     ActivateFinalTarget "KillProcess"

//     let serverProcess =
//         let info = System.Diagnostics.ProcessStartInfo()
//         info.FileName <- dotnetExePath
//         info.WorkingDirectory <- serverPath
//         info.Arguments <- " run"
//         info.UseShellExecute <- false
//         System.Diagnostics.Process.Start info

//     System.Threading.Thread.Sleep 5000 |> ignore  // give server some time to start

//     !! clientTestExecutables
//     |> Expecto (fun p -> { p with Parallel = false } )
//     |> ignore

//     serverProcess.Kill()
// )

// --------------------------------------------------------------------------------------
// Run the Website

let ipAddress = "localhost"
let port = 8080

// FinalTarget "KillProcess" (fun _ ->
//     killProcess "dotnet"
//     killProcess "dotnet.exe"
// )


Target "Run" (fun _ ->
    // let unitTestsWatch = async {
    //     let result =
    //         ExecProcess (fun info ->
    //             info.FileName <- dotnetExePath
    //             info.WorkingDirectory <- serverTestsPath
    //             info.Arguments <- "watch msbuild /t:TestAndRun") TimeSpan.MaxValue

    //     if result <> 0 then failwith "Website shut down." }

    let suave = async { runDotnet serverPath "run" }
    let fablewatch = async { runDotnet clientPath "fable yarn-run start" } // nicht  webpack-dev-server, sonst wird webpack config nicht gefunden
    let openBrowser = async {
        System.Threading.Thread.Sleep(5000)
        Diagnostics.Process.Start("http://"+ ipAddress + sprintf ":%d" port) |> ignore }

    Async.Parallel [|  suave; fablewatch; openBrowser |]
    |> Async.RunSynchronously
    |> ignore
)


// -------------------------------------------------------------------------------------
Target "Build" DoNothing
Target "All" DoNothing

"Clean"
  ==> "InstallDotNetCore"
  ==> "Restore"
  ==> "InstallClient"
  ==> "BuildClient"
  ==> "All"

"BuildClient"
  ==> "Build"

"InstallClient"
  ==> "Run"

"BuildTests"
  ==> "RunTests"

RunTargetOrDefault "All"
