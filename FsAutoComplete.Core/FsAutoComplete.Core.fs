namespace FsAutoComplete

open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices
open FSharpLint.Application
open Microsoft.FSharp.Compiler.Range

module Response = CommandResponse

module Types = 
    type Range = {
        startLine : int
        startCol : int
        endLine : int
        endCol : int
    } with static member Create(sl, sc, el, ec) = {startLine = sl; startCol = sc; endLine = el; endCol = ec}

    type FormatData = 
    | File of filename : string
    | FileSelection of fileName : string * selection : Range

    type FormatConfig = {
        /// Number of spaces for each indentation
        IndentSpaceNum: int
        /// The column where we break to new lines
        PageWidth: int
        SemicolonAtEndOfLine: bool
        SpaceBeforeArgument: bool
        SpaceBeforeColon: bool
        SpaceAfterComma: bool
        SpaceAfterSemicolon: bool
        IndentOnTryWith: bool
        /// Reordering and deduplicating open statements
        ReorderOpenDeclaration: bool
        SpaceAroundDelimiter: bool
        /// Prettyprinting based on ASTs only
        StrictMode: bool
    } with 
    static member Default = {
      IndentSpaceNum = 4; PageWidth = 80;
      SemicolonAtEndOfLine = false; SpaceBeforeArgument = true; SpaceBeforeColon = true;
      SpaceAfterComma = true; SpaceAfterSemicolon = true; 
      IndentOnTryWith = false; ReorderOpenDeclaration = false; 
      SpaceAroundDelimiter = true; StrictMode = false
    }

module Convert = 
  let toFantomasConfig (c : Types.FormatConfig) : Fantomas.FormatConfig.FormatConfig = 
    {
      IndentOnTryWith = c.IndentOnTryWith
      IndentSpaceNum = c.IndentSpaceNum
      PageWidth = c.PageWidth
      ReorderOpenDeclaration = c.ReorderOpenDeclaration
      SemicolonAtEndOfLine = c.SemicolonAtEndOfLine
      SpaceAfterComma = c.SpaceAfterComma
      SpaceAfterSemicolon = c.SpaceAfterSemicolon
      SpaceAroundDelimiter = c.SpaceAroundDelimiter
      SpaceBeforeArgument = c.SpaceBeforeArgument
      SpaceBeforeColon = c.SpaceBeforeColon
      StrictMode = c.StrictMode
    }
  let toFantomasRange (r : Types.Range) = Fantomas.CodeFormatter.MakeRange(r.startLine, r.startCol, r.endLine, r.endCol)

module Commands =
    let parse (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) file lines = async {
        let colorizations = state.ColorizationOutput
        let parse' fileName text options =
            async {
                let! _parseResults, checkResults = checker.ParseAndCheckFileInProject(fileName, 0, text, options)
                return match checkResults with
                        | FSharpCheckFileAnswer.Aborted -> [Response.info serialize "Parse aborted"]
                        | FSharpCheckFileAnswer.Succeeded results ->
                            if colorizations then
                                [ Response.errors serialize (results.Errors)
                                  Response.colorizations serialize (results.GetExtraColorizationsAlternate()) ]
                            else [ Response.errors serialize (results.Errors) ]
            }
        let file = Path.GetFullPath file
        let text = String.concat "\n" lines

        if Utils.isAScript file then
          let checkOptions = checker.GetProjectOptionsFromScript(file, text)
          let state' = state.WithFileTextAndCheckerOptions(file, lines, checkOptions)
          let! res = (parse' file text checkOptions)
          return res , state'
        else
          let state', checkOptions = state.WithFileTextGetCheckerOptions(file, lines)
          let! res = (parse' file text checkOptions)
          return res, state'
    }



    let project (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) file time verbose = async {
        let file = Path.GetFullPath file

        // The FileSystemWatcher often triggers multiple times for
        // each event, as editors often modify files in several steps.
        // This 'debounces' the events, by only reloading a max of once
        // per second.
        return match state.ProjectLoadTimes.TryFind file with
                | Some oldtime when time - oldtime < TimeSpan.FromSeconds(1.0) -> [],state
                | _ ->

                match checker.TryGetProjectOptions(file, verbose) with
                | Result.Failure s -> [Response.error serialize s],state
                | Result.Success(po, projectFiles, outFileOpt, references, logMap) ->
                    let res = Response.project serialize (file, projectFiles, outFileOpt, references, logMap)
                    let checkOptions =
                      projectFiles
                      |> List.fold (fun s f -> Map.add f po s) state.FileCheckOptions
                    let loadTimes = Map.add file time state.ProjectLoadTimes
                    let state' =  { state with FileCheckOptions = checkOptions; ProjectLoadTimes = loadTimes }
                    [res], state'
    }

    let declarations (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) file = async {
        let file = Path.GetFullPath file
        return match state.TryGetFileCheckerOptionsWithSource(file) with
                | Failure s -> [Response.error serialize (s)], state
                | Success (checkOptions, source) ->
                    let decls = checker.GetDeclarations(file, source, checkOptions)
                    [Response.declarations serialize (decls)], state
    }

    let helptext (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) sym = async {
        return match Map.tryFind sym state.HelpText with
                | None -> [Response.error serialize (sprintf "No help text available for symbol '%s'" sym)], state
                | Some tip -> [Response.helpText serialize (sym, tip)], state
    }

    let compilerLocation (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) = async {
        return [Response.compilerLocation serialize Environment.fsc Environment.fsi Environment.msbuild], state
    }

    let colorization (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) enabled = async {
        return [], { state with ColorizationOutput = enabled }
    }

    let error (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) msg = async {
        return [Response.error serialize msg], state
    }

    let completion (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) (tyRes : ParseAndCheckResults ) line col lineStr timeout filter = async {
        return match tyRes.TryGetCompletions line col lineStr timeout filter with
                | Some (decls, residue) ->
                    let declName (d: FSharpDeclarationListItem) = d.Name

                    // Send the first helptext without being requested.
                    // This allows it to be displayed immediately in the editor.
                    let firstMatchOpt =
                      Array.sortBy declName decls
                      |> Array.tryFind (fun d -> (declName d).StartsWith residue)
                    let res = match firstMatchOpt with
                                | None -> [Response.completion serialize (decls)]
                                | Some d ->
                                    [ Response.helpText serialize (d.Name, d.DescriptionText)
                                      Response.completion serialize (decls) ]



                    let helptext =
                      Seq.fold (fun m d -> Map.add (declName d) d.DescriptionText m) Map.empty decls
                    res,{ state with HelpText = helptext }

                | None ->
                    [Response.error serialize "Timed out while fetching completions"], state
    }

    let toolTip  (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) (tyRes : ParseAndCheckResults ) line col lineStr = async {
        // A failure is only info here, as this command is expected to be
        // used 'on idle', and frequent errors are expected.
        return match tyRes.TryGetToolTip line col lineStr with
                | Result.Failure s -> [Response.info serialize (s)], state
                | Result.Success tip -> [Response.toolTip serialize tip], state
    }

    let symbolUse  (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) (tyRes : ParseAndCheckResults ) line col lineStr = async {
        // A failure is only info here, as this command is expected to be
        // used 'on idle', and frequent errors are expected.
        return match tyRes.TryGetSymbolUse line col lineStr with
                | Result.Failure s -> [Response.info serialize (s)], state
                | Result.Success (sym,usages) -> [Response.symbolUse serialize (sym,usages)], state
    }

    let findDeclarations  (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) (tyRes : ParseAndCheckResults ) line col lineStr = async {
        return match tyRes.TryFindDeclaration line col lineStr with
                | Result.Failure s -> [Response.error serialize (s)], state
                | Result.Success range -> [Response.findDeclaration serialize range], state
    }

    let methods (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) (tyRes : ParseAndCheckResults ) line col lines = async {


        return match tyRes.TryGetMethodOverrides lines line col with
                | Result.Failure s -> [Response.error serialize (s)], state
                | Result.Success (meth, commas) -> [Response.methods serialize (meth, commas)], state
    }

    let lint (serialize : obj -> string) (state : State) (checker : FSharpCompilerServiceChecker) file = async {
        let file = Path.GetFullPath file
        let res =
            match state.TryGetFileCheckerOptionsWithSource file with
            | Failure s -> [Response.error serialize (s)], state
            | Success (options,source) ->
            let tyResOpt = checker.TryGetRecentTypeCheckResultsForFile(file, options)
            match tyResOpt with
            | None -> [ Response.info serialize "Cached typecheck results not yet available"], state
            | Some tyRes ->

            match tyRes.GetAST with
            | None -> [ Response.info serialize "Something went wrong during parsing"], state
            | Some tree ->
                let res =
                    Lint.lintParsedSource
                        Lint.OptionalLintParameters.Default
                        { Ast = tree
                          Source = source
                          TypeCheckResults = Some tyRes.GetCheckResults
                          FSharpVersion = Version() }
                let res' =
                    match res with
                    | LintResult.Failure _ -> [ Response.info serialize "Something went wrong during parsing"]
                    | LintResult.Success warnings -> [ Response.lint serialize warnings ]

                res',state
        return res

    }

    let format (serialize : obj -> string) (state  : State) (checker : FSharpChecker) config data =
        let convertedConfig = Convert.toFantomasConfig config
        let formatFile fileName options source = async {
            return! Fantomas.CodeFormatter.FormatDocumentAsync(fileName, source, convertedConfig , options, checker)
        }

        let formatSelection fileName (range : Types.Range) options source = async {
            let frange : Microsoft.FSharp.Compiler.Range.range = Convert.toFantomasRange range
            return! Fantomas.CodeFormatter.FormatSelectionAsync(fileName, frange, source, convertedConfig, options, checker)
        }

        let fileName = 
            match data with 
            | Types.FormatData.File(fileName) -> fileName 
            | Types.FormatData.FileSelection(name, _) -> name

        let file = Path.GetFullPath fileName 
        async {
            match state.TryGetFileCheckerOptionsWithSource file with
            | Failure s -> return [Response.error serialize (s)], state
            | Success(options, source) ->     
                try 
                    match data with
                    | Types.FormatData.File(_) -> 
                        let! res =  formatFile file options source
                        return [Response.format serialize res], state
                    | Types.FormatData.FileSelection(_, range) -> 
                        let! res = formatSelection file range options source
                        return [Response.format serialize res], state
                with 
                | :? Fantomas.FormatConfig.FormatException as e ->
                    return [Response.error serialize e.Message], state
        }
