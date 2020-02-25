namespace ProjectSystem

open System
open System.IO
#if NETSTANDARD2_0
open System.Runtime.InteropServices
#endif
open Dotnet.ProjInfo.Workspace

[<RequireQualifiedAccess>]
module Environment =



  /// Determines if the current system is an Unix system.
  /// See http://www.mono-project.com/docs/faq/technical/#how-to-detect-the-execution-platform
  let isUnix =
  #if NETSTANDARD2_0
      RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
      RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
  #else
      int System.Environment.OSVersion.Platform |> fun p -> (p = 4) || (p = 6) || (p = 128)
  #endif

  /// Determines if the current system is a MacOs system
  let isMacOS =
  #if NETSTANDARD2_0
      RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
  #else
      (System.Environment.OSVersion.Platform = PlatformID.MacOSX) ||
          // osascript is the AppleScript interpreter on OS X
          File.Exists "/usr/bin/osascript"
  #endif

  /// Determines if the current system is a Linux system
  let isLinux =
  #if NETSTANDARD2_0
      RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
  #else
      isUnix && not isMacOS
  #endif

  /// Determines if the current system is a Windows system
  let isWindows =
  #if NETSTANDARD2_0
      RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
  #else
      match System.Environment.OSVersion.Platform with
      | PlatformID.Win32NT | PlatformID.Win32S | PlatformID.Win32Windows | PlatformID.WinCE -> true
      | _ -> false
  #endif


  let runningOnMono =
    try not << isNull <| Type.GetType "Mono.Runtime"
    with _ -> false

  let msbuildLocator = MSBuildLocator()

  let msbuild =
    let msbuildPath = msbuildLocator.LatestInstalledMSBuildNET()

    match msbuildPath with
    | Dotnet.ProjInfo.Inspect.MSBuildExePath.Path path ->
      Some path
    | Dotnet.ProjInfo.Inspect.MSBuildExePath.DotnetMsbuild p ->
      // failwithf "expected msbuild, not 'dotnet %s'" p
      None

  let private environVar v = Environment.GetEnvironmentVariable v

  let private programFilesX86 =
      let wow64 = environVar "PROCESSOR_ARCHITEW6432"
      let globalArch = environVar "PROCESSOR_ARCHITECTURE"
      match wow64, globalArch with
      | "AMD64", "AMD64"
      | null, "AMD64"
      | "x86", "AMD64" -> environVar "ProgramFiles(x86)"
      | _ -> environVar "ProgramFiles"
      |> fun detected -> if detected = null then @"C:\Program Files (x86)\" else detected

  // Below code slightly modified from FAKE MSBuildHelper.fs

  let private vsSkus = ["Community"; "Professional"; "Enterprise"; "BuildTools"]
  let private vsVersions = ["2019"; "2017";]
  let private cartesian a b =
    [ for a' in a do
        for b' in b do
          yield a', b' ]

  let private vsRoots =
    cartesian vsVersions vsSkus
    |> List.map (fun (version, sku) -> programFilesX86 </> "Microsoft Visual Studio" </> version </> sku)

  /// these are the single-instance installation paths on windows from FSharp versions < 4.5
  let private legacyFSharpInstallationPaths =
    ["10.1"; "4.1"; "4.0"; "3.1"; "3.0"]
    |> List.map (fun v -> programFilesX86 </> @"\Microsoft SDKs\F#\" </> v </> @"\Framework\v4.0")

  /// starting with F# 4.5 the binaries are installed in a side-by-side manner to a per-VS-edition folder
  let private sideBySideFSharpInstallationPaths =
    let pattern root = root </> "Common7" </> "IDE" </> "CommonExtensions" </> "Microsoft" </> "FSharp"
    vsRoots |> List.map pattern

  let private fsharpInstallationPath =
    sideBySideFSharpInstallationPaths @ legacyFSharpInstallationPaths
    |> List.tryFind Directory.Exists

  let fsi =
    // on netcore on non-windows we just deflect to fsharpi as usual
    if runningOnMono || not isWindows then Some "fsharpi"
    else
      // if running on windows, non-mono we can't yet send paths to the netcore version of fsi.exe so use the one from full-framework
      fsharpInstallationPath |> Option.map (fun root -> root </> "fsi.exe")

  let fsc =
    if runningOnMono || not isWindows then Some "fsharpc"
    else
      // if running on windows, non-mono we can't yet send paths to the netcore version of fsc.exe so use the one from full-framework
      fsharpInstallationPath |> Option.map (fun root -> root </> "fsc.exe")

  let fsharpCore =
    let dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
    dir </> "FSharp.Core.dll"

  let workspaceLoadDelay () =
    match System.Environment.GetEnvironmentVariable("FSAC_WORKSPACELOAD_DELAY") with
    | delayMs when not (String.IsNullOrWhiteSpace(delayMs)) ->
        match System.Int32.TryParse(delayMs) with
        | true, x -> TimeSpan.FromMilliseconds(float x)
        | false, _ -> TimeSpan.Zero
    | _ -> TimeSpan.Zero

  /// The sdk root that we assume for FSI-ref-location purposes.
  /// TODO: make this settable via ENV variable or explicit LSP config
  let dotnetSDKRoot =
    lazy (
      let fromEnv =
        Environment.GetEnvironmentVariable "DOTNET_ROOT"
        |> Option.ofObj
      defaultArg fromEnv FSIRefs.defaultDotNetSDKRoot
    )

  let private maxVersionWithThreshold (minVersion: FSIRefs.NugetVersion) (versions: FSIRefs.NugetVersion []) =
    versions
    |> Array.filter (fun v -> FSIRefs.compareNugetVersion v minVersion >= 0) // get all versions that compare as greater than the minVersion
    |> Array.sortWith FSIRefs.compareNugetVersion
    |> Array.tryLast

  /// because 3.x is the minimum SDK that we support for FSI, we want to float to the latest
  /// 3.x sdk that the user has installed, to prevent hard-coding.
  let latest3xSdkVersion dotnetRoot =
    let minSDKVersion = FSIRefs.NugetVersion(3,0,100,"")
    lazy (
      match FSIRefs.sdkVersions dotnetRoot with
      | None -> None
      | Some sortedSdkVersions ->
        maxVersionWithThreshold minSDKVersion sortedSdkVersions
    )

  /// because 3.x is the minimum runtime that we support for FSI, we want to float to the latest
  /// 3.x runtime that the user has installed, to prevent hard-coding.
  let latest3xRuntimeVersion dotnetRoot =
    let minRuntimeVersion = FSIRefs.NugetVersion(3,0,0,"")
    lazy (
      match FSIRefs.runtimeVersions dotnetRoot with
      | None -> None
      | Some sortedRuntimeVersions ->
        maxVersionWithThreshold minRuntimeVersion sortedRuntimeVersions
    )
