// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing
open Fake.NuGetHelper

let projectName = "Topshelf.FSharp"

let buildDir = "./.build/"
let deployDir = "./.deploy/"
let testDir = "./.test/"
let project = !! (sprintf "src/%s/*.fsproj" projectName)
let nuspec = sprintf "%s%s.nuspec" buildDir projectName
let packages = !! "./**/packages.config"

printfn "%s" nuspec

Target "Clean" (fun() ->
  CleanDirs [buildDir; deployDir; testDir]
)


Target "RestorePackages" (fun _ ->
  packages
  |> Seq.iter (RestorePackage (fun p -> {p with OutputPath = "./src/packages"}))
)

Target "Build" (fun() ->
//open Fake.AssemblyInfoFile

//Target "BuildApp" (fun _ ->
//    CreateCSharpAssemblyInfo "./src/Topshelf.FSharp/AssemblyInfo.cs"
//        [Attribute.Title "Calculator Command line tool"
//         Attribute.Description "Sample project for FAKE - F# MAKE"
//         Attribute.Guid "43854298-65cd-4b69-a63d-2accb7c68cec"
//         Attribute.Product "Calculator"
//         Attribute.Version version
//         Attribute.FileVersion version]

//Attribute.Metadata("githash", commitHash)

  project
  |> MSBuildRelease buildDir "ResolveReferences;Build"
  |> ignore
)

Target "Package" (fun _ ->
  let versionCandidate = (environVar "version")
  let version = if versionCandidate = "" || versionCandidate = null then "0.0.0" else versionCandidate
  NuGet (fun p ->
        {p with
            Authors = ["Henrik Feldt"; "Tomas Jansson"]
            Project = "test.test"
            Description = "Topshelf F# API"
            OutputPath = deployDir
            WorkingDir = buildDir
            Version = version
            Publish = false
            Files = [
              (@"**\Topshelf.FSharp.*", Some "lib", None)
            ]
        //    References = [projectName + ".dll"]
        })
            nuspec
)

"Clean"
==> "RestorePackages"
==> "Build"
==> "Package"

// start build
RunTargetOrDefault "Package"
