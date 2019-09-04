#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

open Fake.Tools.Git
open Fake.DotNet.Testing

open System.IO

let summary = "The message bus library."
let authors = [ "Mikhail Zabolotko Alex Zaitzev" ]
let configuration = DotNet.BuildConfiguration.Release

let release = ReleaseNotes.parse (File.ReadLines "RELEASE_NOTES.md")

let buildDir = __SOURCE_DIRECTORY__ @@ @"\artifacts\build"
let testDir = __SOURCE_DIRECTORY__ @@ @"\artifacts\tests"
let altcoverDir = __SOURCE_DIRECTORY__ @@ @"\artifacts\altcover"
let tempDirs =
    !! "Sources/**/bin"
    ++ "Tests/**/bin"

Target.create "Clean" (fun _ ->
    Shell.cleanDirs [ buildDir; testDir ]
    tempDirs |> Seq.iter Directory.delete
)

Target.create "GenerateAssemblyInfo" (fun _ ->
    let version = release.SemVer.AsString
    printfn "%A" version

    let now = System.DateTime.Now
    let currentYear = now.Year
    let getAssemblyInfoAttributes projectName =
        [ AssemblyInfo.Title projectName
          AssemblyInfo.Product projectName
          AssemblyInfo.Description summary
          AssemblyInfo.Version version
          AssemblyInfo.FileVersion version
          AssemblyInfo.Company (authors |> String.concat ",")
          AssemblyInfo.Copyright ("(c) Focus Technologies, " + currentYear.ToString())
          AssemblyInfo.Configuration (configuration.ToString()) ]

    let getProjectDetails projectPath =
        let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
        ( System.IO.Path.GetDirectoryName(projectPath), (getAssemblyInfoAttributes projectName))

    !! "Sources/**/*.csproj"
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (folderName, attributes) ->
                    AssemblyInfoFile.createCSharp ((folderName </> "Properties") </> "AssemblyInfo.cs") attributes)
)

Target.create "BuildApp" (fun _ ->
    !! "Kontur.sln"
        |> MSBuild.runRelease id null "Build"
        |> ignore
)

Target.create "KonturTest" (fun _ ->
    let parameters (p : DotNet.TestOptions) =
        { p with
            MSBuildParams =
                { MSBuild.CliArguments.Create() with
                    Properties =
                        [ ("AltCover", "true");
                          ("AltCoverXmlReport", altcoverDir @@ "altcover.xml");
                          ("AltCoverAssemblyExcludeFilter", "NUnit3.TestAdapter|Kontur.Tests") ] } }
    @"Tests\Kontur.Tests\Kontur.Tests.csproj"
    |> DotNet.test parameters
    Shell.Exec ("dotnet", __SOURCE_DIRECTORY__ @@ @".\packages\build\ReportGenerator\tools\netcoreapp2.1\ReportGenerator.dll -reports:.\artifacts\altcover\*.xml -targetdir:.\artifacts\reports", __SOURCE_DIRECTORY__) |> ignore
)

Target.create "KonturRabbitMqTest" (fun _ ->
    @"Tests\Kontur.Rabbitmq.Tests\Kontur.Rabbitmq.Tests.csproj"
    |> DotNet.test id
)

Target.create "KonturRabbitmqIntegrationTest" (fun _ ->
    @"Tests\Kontur.Rabbitmq.IntegrationTests\Kontur.Rabbitmq.IntegrationTests.csproj"
    |> DotNet.test id
)


Target.create "All" ignore

"Clean"
  ==> "GenerateAssemblyInfo"
  ==> "BuildApp"
  ==> "KonturTest"
  ==> "KonturRabbitMqTest"
  ==> "KonturRabbitmqIntegrationTest"
  ==> "All"

Target.runOrDefaultWithArguments "All"
