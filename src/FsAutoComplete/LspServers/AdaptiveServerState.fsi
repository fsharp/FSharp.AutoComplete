namespace FsAutoComplete.Lsp

open System
open FsAutoComplete
open FsAutoComplete.CodeFix
open FsAutoComplete.Logging
open Ionide.LanguageServerProtocol
open Ionide.LanguageServerProtocol.Types

open FsAutoComplete.LspHelpers
open FSharp.UMX
open FSharp.Compiler.Text
open FSharp.Compiler.EditorServices
open FSharp.Data.Adaptive
open Ionide.ProjInfo
open FSharp.Compiler.CodeAnalysis
open IcedTasks
open FsAutoComplete.FCSPatches
open FsAutoComplete.Lsp

[<RequireQualifiedAccess>]
type WorkspaceChosen =
  | Projs of HashSet<string<LocalPath>>
  | NotChosen

[<RequireQualifiedAccess>]
type AdaptiveWorkspaceChosen =
  | Projs of amap<string<LocalPath>, DateTime>
  | NotChosen

[<CustomEquality; NoComparison>]
type LoadedProject =
  { FSharpProjectOptions: FSharpProjectOptions
    LanguageVersion: LanguageVersionShim }

  interface IEquatable<LoadedProject>
  override GetHashCode: unit -> int
  override Equals: other: obj -> bool
  member SourceFiles: string array
  member ProjectFileName: string
  static member op_Implicit: x: LoadedProject -> FSharpProjectOptions

type AdaptiveState =
  new:
    lspClient: FSharpLspClient * sourceTextFactory: ISourceTextFactory * workspaceLoader: IWorkspaceLoader ->
      AdaptiveState

  member RootPath: string option with get, set
  member Config: FSharpConfig with get, set
  member LoadAnalyzers: unit -> unit
  member ClientCapabilities: ClientCapabilities option with get, set
  member WorkspacePaths: WorkspaceChosen with get, set
  member DiagnosticCollections: DiagnosticCollection
  member ScriptFileProjectOptions: Event<FSharpProjectOptions>


  member OpenDocument: filePath: string<LocalPath> * text: string * version: int -> CancellableTask<unit>
  member ChangeDocument: filePath: string<LocalPath> * p: DidChangeTextDocumentParams -> CancellableTask<unit>
  member SaveDocument: filePath: string<LocalPath> * text: string option -> CancellableTask<unit>
  member ForgetDocument: filePath: DocumentUri -> Async<unit>
  member ParseAllFiles: unit -> Async<FSharpParseFileResults option array>
  member GetOpenFile: filePath: string<LocalPath> -> VolatileFile option
  member GetOpenFileSource: filePath: string<LocalPath> -> Async<Result<IFSACSourceText, string>>
  member GetOpenFileOrRead: filePath: string<LocalPath> -> Async<Result<VolatileFile, string>>
  member GetParseResults: filePath: string<LocalPath> -> Async<Result<FSharpParseFileResults, string>>
  member GetOpenFileTypeCheckResults: file: string<LocalPath> -> Async<Result<ParseAndCheckResults, string>>
  member GetOpenFileTypeCheckResultsCached: filePath: string<LocalPath> -> Async<Result<ParseAndCheckResults, string>>
  member GetProjectOptionsForFile: filePath: string<LocalPath> -> Async<Result<FSharpProjectOptions, string>>

  member GetTypeCheckResultsForFile:
    filePath: string<LocalPath> * opts: FSharpProjectOptions -> Async<Result<ParseAndCheckResults, string>>

  member GetTypeCheckResultsForFile: filePath: string<LocalPath> -> Async<Result<ParseAndCheckResults, string>>
  member GetFilesToProject: unit -> Async<(string<LocalPath> * LoadedProject) array>

  member GetUsesOfSymbol:
    filePath: string<LocalPath> *
    opts: (string * FSharpProjectOptions) seq *
    symbol: FSharp.Compiler.Symbols.FSharpSymbol ->
      Async<FSharpSymbolUse array>

  member Codefixes: (CodeActionParams -> Async<Result<Types.Fix list, string>>) array
  member GlyphToCompletionKind: (FSharpGlyph -> Types.CompletionItemKind option)

  member UpdateAutocompleteItems:
    items:
      (DeclName *
      (DeclarationListItem *
      Position *
      string<LocalPath> *
      (Position -> string option) *
      FSharp.Compiler.Syntax.ParsedInput)) list ->
      bool

  member GetAutoCompleteByDeclName:
    declName: DeclName ->
      (DeclarationListItem *
      Position *
      string<LocalPath> *
      (Position -> string option) *
      FSharp.Compiler.Syntax.ParsedInput) option

  member GetAutoCompleteNamespacesByDeclName: declName: DeclName -> CompletionNamespaceInsert option

  member SymbolUseWorkspace:
    includeDeclarations: bool *
    includeBackticks: bool *
    errorOnFailureToFixRange: bool *
    pos: Position *
    lineStr: LineStr *
    text: IFSACSourceText *
    tyRes: ParseAndCheckResults ->
      Async<Result<Collections.Generic.IDictionary<string<LocalPath>, Range array>, string>>

  member GetDeclarationLocation:
    symbolUse: FSharpSymbolUse * text: IFSACSourceText -> Async<Option<SymbolLocation.SymbolDeclarationLocation>>

  member GetDeclarations: filename: string<LocalPath> -> Async<NavigationTopLevelDeclaration array option>
  member GetAllDeclarations: unit -> Async<(string<LocalPath> * NavigationTopLevelDeclaration array) array>
  member GlyphToSymbolKind: (FSharpGlyph -> SymbolKind option)
  interface IDisposable
