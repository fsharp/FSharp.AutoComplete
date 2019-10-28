// include Fake lib
#r @"packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.ReleaseNotesHelper
open System
open System.IO

let project = "FsAutoComplete"

// Read additional information from the release notes document
let releaseNotesData =
    File.ReadAllLines "RELEASE_NOTES.md"
    |> parseAllReleaseNotes

let release = List.head releaseNotesData

let isMono = Fake.EnvironmentHelper.isMono

let configuration = getBuildParamOrDefault "configuration" "Release"

let buildDir = "src" </> project </> "bin" </> "Debug"
let buildReleaseDir = "src" </> project </>  "bin" </> "Release"
let integrationTestDir = "test" </> "FsAutoComplete.IntegrationTests"
let releaseArchive = "bin" </> "pkgs" </> "fsautocomplete.zip"
let releaseArchiveNetCore = "bin" </> "pkgs" </> "fsautocomplete.netcore.zip"

let integrationTests =
  !! (integrationTestDir + "/**/*Runner.fsx")
  -- (integrationTestDir + "/DotNetSdk*/*.*")

type FSACRuntime = NET | NETCoreSCD | NETCoreFDD
type IntegrationTestConfig = {Runtime: FSACRuntime }

let (|AnyNetcoreRuntime|_|) r =
  match r with
  | FSACRuntime.NETCoreSCD
  | FSACRuntime.NETCoreFDD -> Some ()
  | FSACRuntime.NET -> None

let isTestSkipped cfg (fn: string) =
  let file = Path.GetFileName(fn)
  let dir = Path.GetFileName(Path.GetDirectoryName(fn))

  match cfg.Runtime, dir, file with
  // known difference. On mono the error message from msbuild is different and not normalized
  | _, "OldSdk", "InvalidProjectFileRunner.fsx" when isMono ->
    Some "known difference. On mono the error message from msbuild is different and not normalized"
  // stdio and http
  | _, "Test1Json", "Test1JsonRunner.fsx" ->
    Some "flaky, the Range sometimes finish at start of newline other times at end of line"
  | _, "ProjectCache", "Runner.fsx" ->
    Some "fails, ref https://github.com/fsharp/FsAutoComplete/issues/198"
  | _, "DotNetSdk2.0CrossgenWithNetFx", "Runner.fsx" ->
    match isWindows, environVar "FSAC_TESTSUITE_CROSSGEN_NETFX" with
    | true, _ -> None //always run it on windows
    | false, "1" -> None //force run on mono
    | false, _ -> Some "not supported on this mono version" //by default skipped on mono
//  | _, _, "DotNetSdk2.0", "InvalidProjectFileRunner.fsx"
  | AnyNetcoreRuntime, "OldSdk", "InvalidProjectFileRunner.fsx" when not(isWindows) ->
    Some "the regex to normalize output fails. mono/.net divergence?" //by default skipped on mono
  // .net core based fsac
  | AnyNetcoreRuntime, "NoFSharpCoreReference", "Runner.fsx" ->
    Some "know failure, the FSharp.Core is not added if not in the fsc args list"
  // known difference, the FSharp.Core of script is different so are xmldoc
  | AnyNetcoreRuntime, "Tooltips", "Runner.fsx" ->
    Some "known difference, the FSharp.Core of script is different so are xmldoc"
  // by default others are enabled
  | _ -> None

let runIntegrationTest cfg (fn: string) : bool =
  let dir = Path.GetDirectoryName fn

  match isTestSkipped cfg fn with
  | Some msg ->
    tracefn "Skipped '%s' reason: %s"  fn msg
    true
  | None ->
    let framework =
      match cfg.Runtime with
      | FSACRuntime.NET -> "net461"
      | FSACRuntime.NETCoreSCD
      | FSACRuntime.NETCoreFDD -> "netcoreapp2.1"
    let fsiArgs = sprintf "%s -- -pub -f %s -c %s" fn framework configuration
    let fsiPath = FSIHelper.fsiPath
    tracefn "Running fsi '%s %s' (from dir '%s')"  fsiPath fsiArgs dir
    let testExecution =
      try
        FileUtils.pushd dir

        let result, messages =
            ExecProcessRedirected (fun info ->
              info.FileName <- fsiPath
              info.Arguments <- fsiArgs
              info.WorkingDirectory <- dir
            ) (TimeSpan.FromMinutes(1.0))

        System.Threading.Thread.Sleep (TimeSpan.FromSeconds(1.0))

        Some (result, messages |> List.ofSeq)
      with ex ->
        tracefn "fsi failed with ex %A" ex
        None
    FileUtils.popd ()
    match testExecution with
    | None -> //timeout
      false
    | Some (result, msgs) ->
      let msgs = msgs |> List.filter (fun x -> x.IsError)
      if not result then
        for msg in msgs do
          traceError msg.Message
        let isWebEx = msgs |> List.exists (fun m -> m.Message.Contains("System.Net.WebException"))
        isWebEx
      else
        true

let listAll cfg =
  let willRun, willSkip =
    integrationTests
    |> Seq.map (fun test -> test, isTestSkipped cfg test)
    |> List.ofSeq
    |> List.partition (fun (test, skipped) -> match skipped with
                                              | Some txt -> false
                                              | None -> true)

  printfn "=== Tests to Run ==="
  for (testName, _msg) in willRun do
    printfn "\t%s" testName

let runall cfg =

    trace "Cleanup test dir (git clean)..."
    let clean =
      let ok, out, err =
        Git.CommandHelper.runGitCommand (Path.Combine(__SOURCE_DIRECTORY__, integrationTestDir)) "clean -xdf"
      out |> Seq.iter (printfn "%s")
      printfn "Done: %s" (ok.ToString())

    trace "Resetting output files in test dir (git reset)..."
    let clean =
      let ok, out, err =
        Git.CommandHelper.runGitCommand "." (sprintf "git checkout -- %s" integrationTestDir)
      out |> Seq.iter (printfn "%s")
      printfn "Done: %s" (ok.ToString())

    trace "Running Integration tests..."
    let runOk =
     integrationTests
     |> Seq.map (runIntegrationTest cfg)
     |> Seq.forall id

    if not runOk then
      trace "Integration tests did not run successfully"
      failwith "Integration tests did not run successfully"
    else
      trace "checking tests results..."
      let ok, out, err =
        Git.CommandHelper.runGitCommand
                          "."
                          ("-c core.fileMode=false diff --exit-code " + integrationTestDir)
      if not ok then
        trace (toLines out)
        failwithf "Integration tests failed:\n%s" err
    trace "Done Integration tests."

Target "IntegrationTestStdioMode" (fun _ ->
  ignore()
  // not doing these tests because they need to be migrated to LSP
  // trace "== Integration tests (stdio/net) =="
  // let cfg = { Runtime = NET }
  // listAll cfg
  // runall cfg
)


Target "IntegrationTestStdioModeNetCore" (fun _ ->
  ignore ()
  // not doing these tests because they need to be migrated to LSP
  // trace "== Integration tests (stdio/netcore) =="
  // let cfg = { Runtime = NETCoreFDD }
  // listAll cfg
  // runall cfg
)


Target "LspTest" (fun _ ->
  DotNetCli.RunCommand
      (fun p ->
          { p with
              TimeOut = TimeSpan.FromMinutes 15. })
      """run -c Release --no-build -p "./test/FsAutoComplete.Tests.Lsp/FsAutoComplete.Tests.Lsp.fsproj" -- --fail-on-focused-tests --debug"""
)

Target "ReleaseArchive" (fun _ ->
    CleanDirs [ "bin/pkgs" ]
    ensureDirectory "bin/pkgs"

    !! "bin/release/**/*"
    |> Zip "bin/release" releaseArchive

    !! "bin/release_netcore/**/*"
    |> Zip "bin/release_netcore" releaseArchiveNetCore

    !! (sprintf "bin/release_as_tool/fsautocomplete.%s.nupkg" release.AssemblyVersion)
    |> Copy "bin/pkgs"
)

Target "LocalRelease" (fun _ ->
    ensureDirectory "bin/release"
    CleanDirs [ "bin/release"; "bin/release_netcore" ]

    DotNetCli.Publish (fun p ->
       { p with
           Output = __SOURCE_DIRECTORY__ </> "bin/release"
           Framework = "net461"
           Project = "src/FsAutoComplete"
           Configuration = configuration
           AdditionalArgs = [ "/p:SourceLinkCreate=true"; sprintf "/p:Version=%s" release.AssemblyVersion ]  })

    CleanDirs [ "bin/release_netcore" ]
    DotNetCli.Publish (fun p ->
       { p with
           Output = __SOURCE_DIRECTORY__ </> "bin/release_netcore"
           Framework = "netcoreapp2.1"
           Project = "src/FsAutoComplete"
           Configuration = configuration
           AdditionalArgs = [ "/p:SourceLinkCreate=true"; sprintf "/p:Version=%s" release.AssemblyVersion ]  })

    CleanDirs [ "bin/release_as_tool" ]
    DotNetCli.Pack (fun p ->
       { p with
           OutputPath = __SOURCE_DIRECTORY__ </> "bin/release_as_tool"
           Project = "src/FsAutoComplete"
           Configuration = configuration
           AdditionalArgs = [ "/p:SourceLinkCreate=true"; sprintf "/p:Version=%s" release.AssemblyVersion; "/p:PackAsTool=true" ]  })
)

Target "Clean" (fun _ ->
  CleanDirs [ buildDir; buildReleaseDir ]
  DeleteFiles [ releaseArchive; releaseArchiveNetCore ]
)

Target "Build" (fun _ ->
  DotNetCli.Build (fun p ->
     { p with
         Project = "FsAutoComplete.sln"
         Configuration = configuration
         AdditionalArgs = [ "/p:SourceLinkCreate=true"; sprintf "/p:Version=%s" release.AssemblyVersion ] })
)

Target "Test" id
Target "IntegrationTest" id
Target "All" id
Target "Release" id
Target "BuildDebug" id

"BuildDebug"
  ==> "Build"
  ==> "IntegrationTest"

"BuildDebug"
  ==> "Build"
  ==> "LspTest"

"LocalRelease" ==> "IntegrationTestStdioMode" ==> "IntegrationTest"
"LocalRelease" ==> "IntegrationTestStdioModeNetCore" ==> "IntegrationTest"

"LspTest" ==> "Test"
"IntegrationTest" ==> "Test"
"Test" ==> "All"
"BuildDebug" ==> "All"

"Build"
  ==> "LocalRelease"
  ==> "ReleaseArchive"
  ==> "Release"

"ReleaseArchive" ==> "All"

RunTargetOrDefault "Build"
