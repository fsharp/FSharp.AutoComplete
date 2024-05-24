module FsAutoComplete.CodeFix.AddMissingFunKeyword

open FsToolkit.ErrorHandling
open FsAutoComplete.CodeFix.Navigation
open FsAutoComplete.CodeFix.Types
open Ionide.LanguageServerProtocol.Types
open FsAutoComplete
open FsAutoComplete.LspHelpers

let title = "Add missing 'fun' keyword"

/// a codefix that adds a missing 'fun' keyword to a lambda
let fix (getFileLines: GetFileLines) (getLineText: GetLineText) : CodeFix =
  Run.ifDiagnosticByCode (Set.ofList [ "10" ]) (fun diagnostic codeActionParams ->
    asyncResult {
      let fileName = codeActionParams.TextDocument.GetFilePath() |> Utils.normalizePath

      let! lines = getFileLines fileName
      let! errorText = getLineText lines diagnostic.Range
      do! Result.guard (fun _ -> errorText = "->") "Expected error source code text not matched"

      let! lineLen =
        lines.GetLineLength(protocolPosToPos diagnostic.Range.Start)
        |> Result.ofOption (fun _ -> "Could not get line length")

      let! line =
        getLineText
          lines
          { Start =
              { diagnostic.Range.Start with
                  Character = 0u }
            End =
              { diagnostic.Range.End with
                  Character = uint32 lineLen } }

      let! prevPos =
        dec lines diagnostic.Range.Start
        |> Result.ofOption (fun _ -> "previous position wasn't valid")

      let adjustedPos =
        walkBackUntilCondition lines prevPos (System.Char.IsWhiteSpace >> not)

      match adjustedPos with
      | None -> return []
      | Some firstNonWhitespacePos ->
        let fcsPos = protocolPosToPos firstNonWhitespacePos

        match Lexer.getSymbol fcsPos.Line fcsPos.Column line SymbolLookupKind.Fuzzy [||] with
        | Some lexSym ->
          let fcsStartPos = FSharp.Compiler.Text.Position.mkPos lexSym.Line lexSym.LeftColumn

          let symbolStartRange = fcsPosToProtocolRange fcsStartPos

          return
            [ { Title = title
                File = codeActionParams.TextDocument
                SourceDiagnostic = Some diagnostic
                Edits =
                  [| { Range = symbolStartRange
                       NewText = "fun " } |]
                Kind = FixKind.Fix } ]
        | None -> return []
    })
