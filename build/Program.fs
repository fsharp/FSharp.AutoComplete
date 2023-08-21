open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.Core.TargetOperators
open Fake.Tools

System.Environment.CurrentDirectory <- (Path.combine __SOURCE_DIRECTORY__ "..")

let project = "FsAutoComplete"
let changeLogFile = "CHANGELOG.md"
let mutable changelogs = Changelog.load changeLogFile
let mutable currentRelease = changelogs.LatestEntry

let configuration = Environment.environVarOrDefault "configuration" "Release"

let buildDir = "src" </> project </> "bin" </> "Debug"
let buildReleaseDir = "src" </> project </> "bin" </> "Release"
let pkgsDir = "bin" </> "pkgs"
let toolsDir = "bin" </> "release_as_tool"
let releaseArchiveNetCore = pkgsDir </> "fsautocomplete.netcore.zip"

// Files to format
let sourceFiles =
  !! "src/**/*.fs" ++ "src/**/*.fsi" ++ "build.fsx" -- "src/**/obj/**/*.fs"

type SemverBump =
  | Major
  | Minor
  | Patch

  static member Combine l r =
    match l, r with
    | Major, _
    | _, Major -> Major
    | Minor, _
    | _, Minor -> Minor
    | _ -> Patch

let init args =
  let execContext = Context.FakeExecutionContext.Create false "build.fsx" args
  Context.setExecutionContext (Context.RuntimeContext.Fake execContext)
  Target.initEnvironment ()

  let fsacAssemblies = "FsAutoComplete|FsAutoComplete.Core|LanguageServerProtocol"

  let packAsToolProp = "PackAsTool", "true"

  Target.create "LspTest" (fun _ ->

    let msbuildCli: Fake.DotNet.MSBuild.CliArguments =
      { MSBuild.CliArguments.Create() with
          Properties =
            [ "AltCover", "true"
              // "AltCoverAssemblyFilter", fsacAssemblies
              "AltCoverAssemblyExcludeFilter",
              "System.Reactive|FSharp.Compiler.Service|Ionide.ProjInfo|FSharp.Analyzers|Analyzer|Humanizer|FSharp.Core|FSharp.DependencyManager|FsAutoComplete.Tests.Lsp" ] }

    let testOpts (opts: DotNet.TestOptions) =
      { opts with
          Configuration = DotNet.BuildConfiguration.Release
          RunSettingsArguments = Some("Expecto.fail-on-focused-tests=true")
          MSBuildParams = msbuildCli }

    DotNet.test testOpts "./test/FsAutoComplete.Tests.Lsp/FsAutoComplete.Tests.Lsp.fsproj")

  Target.create "Coverage" (fun _ ->
    DotNet.exec
      id
      "reportgenerator"
      "-reports:test/FsAutoComplete.Tests.Lsp/coverage.xml -reporttypes:Html;HtmlSummary -targetdir:./coverage"
    |> fun r ->
         if not r.OK then
           failwithf "Errors while generating coverage report: %A" r.Errors)

  Target.create "ReleaseArchive" (fun _ ->
    Directory.ensure pkgsDir

    !!(toolsDir </> "fsautocomplete.*.nupkg")
    |> Shell.copy pkgsDir)

  Target.create "LocalRelease" (fun _ ->
    Directory.ensure toolsDir
    Shell.cleanDirs [ toolsDir ]

    DotNet.pack
      (fun p ->
        { p with
            OutputPath = Some(__SOURCE_DIRECTORY__ </> ".." </> toolsDir)
            Configuration = DotNet.BuildConfiguration.fromString configuration
            MSBuildParams = { MSBuild.CliArguments.Create() with Properties = [ packAsToolProp ] } })
      "src/FsAutoComplete")

  Target.create "Clean" (fun _ -> Shell.cleanDirs [ buildDir; buildReleaseDir; pkgsDir; toolsDir ])

  Target.create "Restore" (fun _ -> DotNet.restore id "")

  Target.create "Build" (fun _ ->
    DotNet.build
      (fun p -> { p with Configuration = DotNet.BuildConfiguration.fromString configuration })
      "FsAutoComplete.sln")

  Target.create "EnsureRepoConfig" (fun _ ->
    // Configure custom git hooks
    // * Currently only used to ensure that code is formatted before pushing
    Git.CommandHelper.gitCommand "" "config core.hooksPath .githooks")

  Target.create "NoOp" ignore
  Target.create "Test" ignore
  Target.create "All" ignore
  Target.create "Release" ignore

  let determineBump (currentBump: SemverBump) (c: Changelog.Change) =
    let thisChange =
      match c with
      | Changelog.Change.Added _ -> Minor
      | Changelog.Change.Removed _ -> Major
      | Changelog.Change.Changed _
      | Changelog.Change.Custom _ // TODO: handle?
      | Changelog.Change.Deprecated _
      | Changelog.Change.Fixed _
      | Changelog.Change.Security _ -> Patch

    SemverBump.Combine currentBump thisChange

  let bumpVersion (ver: SemVerInfo) bump =
    match bump with
    | Major ->
      { ver with
          Major = ver.Major + 1u
          Minor = 0u
          Patch = 0u
          PreRelease = None
          Original = None }
    | Minor ->
      { ver with
          Minor = ver.Minor + 1u
          Patch = 0u
          PreRelease = None
          Original = None }
    | Patch ->
      { ver with
          Patch = ver.Patch + 1u
          PreRelease = None
          Original = None }

  Target.create "PromoteUnreleasedToVersion" (fun _ ->
    match changelogs.Unreleased with
    | None -> failwith "No unreleased changes to be promoted"
    | Some unreleased ->
      let nextReleaseNumber =
        Trace.tracefn $"Determining bump for version %O{currentRelease.SemVer}"

        let bump = (Minor, unreleased.Changes) ||> List.fold determineBump

        Trace.tracefn $"Bump type is %O{bump}"
        bumpVersion changelogs.LatestEntry.SemVer bump

      Trace.tracefn $"Promoting unreleased changes to version {nextReleaseNumber}"
      changelogs <- Changelog.promoteUnreleased (string nextReleaseNumber) changelogs
      changelogs |> Changelog.save changeLogFile
      currentRelease <- changelogs.LatestEntry)

  Target.create "CreateVersionTag" (fun _ ->
    Git.Staging.stageFile "." changeLogFile |> ignore<_>

    Git.Commit.exec "." $"Promote changelog entry for %O{currentRelease.SemVer}"
    Git.CommandHelper.gitCommand "." $"tag v%O{currentRelease.SemVer}")

  Target.create "Promote" ignore

  "PromoteUnreleasedToVersion" ==> "CreateVersionTag" ==> "Promote"
  |> ignore<string>

  "Restore" ==> "Build" |> ignore<string>

  "Build" ==> "LspTest" ==> "Coverage" ==> "Test" ==> "All"
  |> ignore<string>

  "Clean"
  ==> "LocalRelease"
  ==> "ReleaseArchive"
  ==> "Release"
  |> ignore<string>

  "ReleaseArchive" ==> "All" |> ignore<string>

[<EntryPoint>]
let main args =
  init ((args |> List.ofArray))

  try
    Target.runOrDefaultWithArguments "ReleaseArchive"

    0
  with e ->
    printfn "%A" e
    1
