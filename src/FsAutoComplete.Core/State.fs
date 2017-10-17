﻿namespace FsAutoComplete

open System
open Microsoft.FSharp.Compiler.SourceCodeServices
open System.Collections.Concurrent
open System.Threading

type DeclName = string

type State =
  {
    Files : ConcurrentDictionary<SourceFilePath, VolatileFile>
    FileCheckOptions : ConcurrentDictionary<SourceFilePath, FSharpProjectOptions>
    Projects : ConcurrentDictionary<ProjectFilePath, Project>
    HelpText : ConcurrentDictionary<DeclName, FSharpToolTipText>
    NavigationDeclarations : ConcurrentDictionary<SourceFilePath, FSharpNavigationTopLevelDeclaration[]>
    CancellationTokens: ConcurrentDictionary<SourceFilePath, CancellationTokenSource list>
    mutable ColorizationOutput: bool
  }

  static member Initial =
    { Files = ConcurrentDictionary()
      FileCheckOptions = ConcurrentDictionary()
      Projects = ConcurrentDictionary()
      HelpText = ConcurrentDictionary()
      CancellationTokens = ConcurrentDictionary()
      NavigationDeclarations = ConcurrentDictionary()
      ColorizationOutput = false }

  member x.GetCheckerOptions(file: SourceFilePath, lines: LineStr[]) : FSharpProjectOptions option =
    let file = Utils.normalizePath file

    x.FileCheckOptions.TryFind file
    |> Option.map (fun opts ->
        x.Files.[file] <- { Lines = lines; Touched = DateTime.Now }
        x.FileCheckOptions.[file] <- opts
        opts
    )

  member x.AddFileTextAndCheckerOptions(file: SourceFilePath, lines: LineStr[], opts) =
    let file = Utils.normalizePath file
    let fileState = { Lines = lines; Touched = DateTime.Now }
    x.Files.[file] <- fileState
    x.FileCheckOptions.[file] <- opts

  member x.AddCancellationToken(file : SourceFilePath, token: CancellationTokenSource) =
    x.CancellationTokens.AddOrUpdate(file, [token], fun _ lst -> token::lst)
    |> ignore

  member x.GetCancellationTokens(file : SourceFilePath) =
    let lst = x.CancellationTokens.GetOrAdd(file, fun _ -> [])
    x.CancellationTokens.TryRemove(file) |> ignore
    lst

  static member private FileWithoutProjectOptions(file) =
    let opts=
        defaultArg (Environment.fsharpCoreOpt  |> Option.map (fun path -> [| yield sprintf "-r:%s" path; yield "--noframework" |] )) [|"--noframework"|]

    { ProjectFileName = file + ".fsproj"
      SourceFiles = [|file|]
      OtherOptions = opts // "--noframework"
      ReferencedProjects = [| |]
      IsIncompleteTypeCheckEnvironment = true
      UseScriptResolutionRules = false
      LoadTime = DateTime.Now
      UnresolvedReferences = None
      OriginalLoadReferences = []
      ExtraProjectInfo = None
      Stamp = None}

  member x.TryGetFileCheckerOptionsWithLines(file: SourceFilePath) : Result<FSharpProjectOptions * LineStr[]> =
    let file = Utils.normalizePath file
    match x.Files.TryFind(file) with
    | None -> Failure (sprintf "File '%s' not parsed" file)
    | Some (volFile) ->

      match x.FileCheckOptions.TryFind(file) with
      | None -> Success (State.FileWithoutProjectOptions(file), volFile.Lines)
      | Some opts -> Success (opts, volFile.Lines)

  member x.TryGetFileCheckerOptionsWithSource(file: SourceFilePath) : Result<FSharpProjectOptions * string> =
    let file = Utils.normalizePath file
    match x.TryGetFileCheckerOptionsWithLines(file) with
    | Failure x -> Failure x
    | Success (opts, lines) -> Success (opts, String.concat "\n" lines)

  member x.TryGetFileCheckerOptionsWithLinesAndLineStr(file: SourceFilePath, pos : Pos) : Result<FSharpProjectOptions * LineStr[] * LineStr> =
    let file = Utils.normalizePath file
    match x.TryGetFileCheckerOptionsWithLines(file) with
    | Failure x -> Failure x
    | Success (opts, lines) ->
      let ok = pos.Line <= lines.Length && pos.Line >= 1 &&
               pos.Col <= lines.[pos.Line - 1].Length + 1 && pos.Col >= 1
      if not ok then Failure "Position is out of range"
      else Success (opts, lines, lines.[pos.Line - 1])
