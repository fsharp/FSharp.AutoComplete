// --------------------------------------------------------------------------------------
// (c) Robin Neatherway
// --------------------------------------------------------------------------------------
namespace FsAutoComplete

open System
open Serilog
open Serilog.Core
open Serilog.Events

module Options =
  open Argu

  type CLIArguments =
      | Version
      | [<AltCommandLine("-v")>] Verbose
      | AttachDebugger
      | [<EqualsAssignment; AltCommandLine("-l")>] Logfile of path:string
      | VFilter of filter:string
      | [<CustomCommandLine("--wait-for-debugger")>] WaitForDebugger
      | [<EqualsAssignment; CustomCommandLine("--hostPID")>] HostPID of pid:int
      | [<CustomCommandLine("--background-service-enabled")>] BackgroundServiceEnabled
      with
          interface IArgParserTemplate with
              member s.Usage =
                  match s with
                  | Version -> "display versioning information"
                  | AttachDebugger -> "launch the system debugger and break."
                  | Verbose -> "enable verbose mode"
                  | Logfile _ -> "send verbose output to specified log file"
                  | VFilter _ -> "apply a comma-separated {FILTER} to verbose output"
                  | WaitForDebugger _ -> "wait for a debugger to attach to the process"
                  | HostPID _ -> "the Host process ID."
                  | BackgroundServiceEnabled -> "enable background service"

  let isCategory (category: string) (e: LogEvent) =
    match e.Properties.TryGetValue "SourceContext" with
    | true, loggerName ->
      match loggerName with
      | :? ScalarValue as v ->
        match v.Value with
        | :? string as s when s = category -> true
        | _ -> false
      | _ -> false
    | false,  _ -> false

  let hasMinLevel (minLevel: LogEventLevel) (e: LogEvent) =
    e.Level >= minLevel

  // will use later when a mapping-style config of { "category": "minLevel" } is established
  let excludeByLevelWhenCategory category level event = isCategory category event || not (hasMinLevel level event)

  let apply (levelSwitch: LoggingLevelSwitch) (logConfig: Serilog.LoggerConfiguration) (args: ParseResults<CLIArguments>) =

    let applyArg arg =
      match arg with
      | Verbose ->
          levelSwitch.MinimumLevel <- LogEventLevel.Verbose
          ()
      | AttachDebugger ->
          System.Diagnostics.Debugger.Launch() |> ignore<bool>
      | Logfile s ->
          try
            logConfig.WriteTo.Async(fun c -> c.File(path = s, levelSwitch = levelSwitch) |> ignore) |> ignore
          with
          | e ->
            printfn "Bad log file: %s" e.Message
            exit 1
      | VFilter v ->
          let filters = v.Split([|','|], StringSplitOptions.RemoveEmptyEntries)
          filters
          |> Array.iter (fun category ->
            // category is encoded in the SourceContext property, so we filter messages based on that property's value
            logConfig.Filter.ByExcluding(Func<_,_>(isCategory category)) |> ignore
          )
      | Version
      | WaitForDebugger
      | BackgroundServiceEnabled
      | HostPID _  ->
          ()

    args.GetAllResults()
    |> List.iter applyArg
