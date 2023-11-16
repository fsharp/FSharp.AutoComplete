﻿// --------------------------------------------------------------------------------------
// (c) Robin Neatherway
// --------------------------------------------------------------------------------------
namespace FsAutoComplete

/// loggers that are shared between components
[<RequireQualifiedAccess>]
module Loggers =
  open FsAutoComplete.Logging

  let analyzers = LogProvider.getLoggerByName "Analyzers"


[<RequireQualifiedAccess>]
module Debug =
  open System
  open System.Diagnostics
  open System.Collections.Concurrent
  open FsAutoComplete.Logging

  let measureLogger = LogProvider.getLoggerByName "Measurer"

  let measure name f =
    let sw = Stopwatch.StartNew()
    let r = f ()
    let elapsed = sw.ElapsedMilliseconds

    measureLogger.info (
      Log.setMessage ("{name} took {elapsed} ms")
      >> Log.addContextDestructured "name" name
      >> Log.addContextDestructured "elapsed" elapsed
    )

    r

  let measureAsync name f =
    async {
      let sw = Stopwatch.StartNew()
      let! r = f
      let elapsed = sw.ElapsedMilliseconds

      measureLogger.info (
        Log.setMessage ("{name} took {elapsed} ms")
        >> Log.addContextDestructured "name" name
        >> Log.addContextDestructured "elapsed" elapsed
      )

      return r
    }

  let toggleVerboseLogging (_verbose: bool) = () // todo: set logging latch

  let waitForDebugger () =
    while not (Diagnostics.Debugger.IsAttached) do
      System.Threading.Thread.Sleep(100)

  let logger = LogProvider.getLoggerByName "Debugging"

  let waitForDebuggerAttached
#if DEBUG
    programName
#else
    _
#endif
    =
#if DEBUG
    if not (System.Diagnostics.Debugger.IsAttached) then
      logger.info (
        Log.setMessage (
          sprintf
            "Please attach a debugger for %s, PID: %d"
            programName
            (System.Diagnostics.Process.GetCurrentProcess().Id)
        )
      )

    while not (System.Diagnostics.Debugger.IsAttached) do
      System.Threading.Thread.Sleep(100)
#else
    ()
#endif
  let waitForDebuggerAttachedAndBreak
#if DEBUG
    programName
#else
    _
#endif
    =
#if DEBUG
    waitForDebuggerAttached programName
    System.Diagnostics.Debugger.Break()
#else
    ()
#endif
