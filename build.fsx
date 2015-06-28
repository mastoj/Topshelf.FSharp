// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open System
open Fake
open Fake.Testing
open Fake.NuGetHelper
open Fake.AssemblyInfoFile
open Fake.Git

let description = "Topshelf F# API"
let projectName = "Topshelf.FSharp"
let authors = ["Henrik Feldt"; "Tomas Jansson"]
let buildDir = "./.build/"
let deployDir = "./.deploy/"
let testDir = "./.test/"
let project = !! (sprintf "src/%s/*.fsproj" projectName)
let nuspec = sprintf "%s%s.nuspec" buildDir projectName
let packages = !! "./**/packages.config"
let versionCandidate = (environVar "version")
let version = if versionCandidate = "" || versionCandidate = null then "0.0.0" else versionCandidate

Target "Clean" (fun() ->
  CleanDirs [buildDir; deployDir; testDir]
)

Target "RestorePackages" (fun _ ->
  packages
  |> Seq.iter (RestorePackage (fun p -> {p with OutputPath = "./src/packages"}))
)

Target "Build" (fun() ->
  let commitHash = Information.getCurrentHash()
  trace "Updating AssemblyVersionInfo"
  CreateFSharpAssemblyInfo "./src/Topshelf.FSharp/AssemblyVersionInfo.fs"
      [Attribute.Title description
       Attribute.Description description
       Attribute.Guid "43854298-65cd-4b69-a63d-2accb7c68cec"
       Attribute.Product projectName
       Attribute.Version version
       Attribute.FileVersion version
       Attribute.Copyright (sprintf "(c) %i by %s" DateTime.Now.Year authors.[0])
       Attribute.Metadata("githash", commitHash)]

  project
  |> MSBuildRelease buildDir "ResolveReferences;Build"
  |> ignore
)

Target "Package" (fun _ ->
  NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = "Topshelf F# API"
            OutputPath = deployDir
            WorkingDir = buildDir
            Version = version
            Publish = false
            Files =
              [ (@"**\Topshelf.FSharp.*", Some "lib", None) ]
        })
        nuspec
)

"Clean"
==> "RestorePackages"
==> "Build"
==> "Package"

// start build
RunTargetOrDefault "Package"
