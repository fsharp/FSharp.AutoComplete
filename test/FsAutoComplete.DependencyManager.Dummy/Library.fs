﻿module FsAutoComplete.DependencyManager.Dummy

open System

/// A marker attribute to tell FCS that this assembly contains a Dependency Manager, or
/// that a class with the attribute is a DependencyManager
[<AttributeUsage(AttributeTargets.Assembly ||| AttributeTargets.Class, AllowMultiple = false)>]
type DependencyManagerAttribute() =
  inherit Attribute()

[<assembly: DependencyManager>]
do ()

/// returned structure from the ResolveDependencies method call.
type ResolveDependenciesResult
  (
    success: bool,
    stdOut: string array,
    stdError: string array,
    resolutions: string seq,
    sourceFiles: string seq,
    roots: string seq
  ) =

  /// Succeeded?
  member __.Success = success

  /// The resolution output log
  member __.StdOut = stdOut

  /// The resolution error log (* process stderror *)
  member __.StdError = stdError

  /// The resolution paths (will be treated as #r options)
  member __.Resolutions = resolutions

  /// The source code file paths (will be treated as #load options)
  member __.SourceFiles = sourceFiles

  /// The roots to package directories (will be treated like #I options)
  member __.Roots = roots

type ScriptExtension = string
type HashRLines = string seq
type TFM = string

/// the type _must_ take an optional output directory
[<DependencyManager>]
type DummyDependencyManager(_outputDir: string option) =

  /// Name of the dependency manager
  member val Name = "Dummy Dependency Manager" with get

  /// Key that identifies the types of dependencies that this DependencyManager operates on
  member val Key = "dummy" with get

  /// Resolve the dependencies, for the given set of arguments, go find the .dll references, scripts and additional include values.
  member _.ResolveDependencies(_scriptExt: ScriptExtension, _includeLines: HashRLines, _tfm: TFM) : obj =
    // generally, here we'd parse the includeLines to determine what to do,
    // package those results into a `ResolveDependenciesResult`,
    // and return it boxed as obj.
    // but here we will return a dummy
    ResolveDependenciesResult(
      true,
      [| "Skipped processing of any hash-r references" |],
      [||],
      Seq.empty,
      Seq.empty,
      Seq.empty
    )
    :> _
