// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing.NUnit3

// Directories
let buildDir  = "./build/"
let deployDir = "./deploy/"
let testDir = "./tests/"

let nunitRunnerPath = "packages/NUnit.ConsoleRunner/tools/nunit3-console.exe"

// Filesets
let appReferences  =
    !! "/**/*.csproj"
    ++ "/**/*.fsproj"

// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "Build" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
    |> Log "AppBuild-Output: "
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*")
    -- "*.zip"
    |> Zip buildDir (deployDir + "ApplicationName." + version + ".zip")
)

Target "BuildTests" (fun _ ->
    !! "src/**/*.Tests.fsproj"
    |> MSBuildDebug testDir "Build"
    |> Log "BuildTests-Output: "
)

Target "RunUnitTests" (fun _ ->
    !! (testDir + "*.Tests.dll")
    |> NUnit3 (fun p ->
                      {p with ToolPath = nunitRunnerPath})
)

// Build order
"Clean"
  ==> "Build"
  ==> "BuildTests"
  ==> "RunUnitTests"
  ==> "Deploy"

// start build
RunTargetOrDefault "RunUnitTests"
