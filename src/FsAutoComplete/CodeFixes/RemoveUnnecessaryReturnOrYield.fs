module FsAutoComplete.CodeFix.RemoveUnnecessaryReturnOrYield

open System
open FsToolkit.ErrorHandling
open FsAutoComplete.CodeFix.Types
open Ionide.LanguageServerProtocol.Types
open FsAutoComplete
open FsAutoComplete.LspHelpers

let title keyword = $"Remove '%s{keyword}'"

/// a codefix that removes 'return' or 'yield' (or bang-variants) when the compiler says they're not necessary
let fix (getParseResultsForFile: GetParseResultsForFile) (getLineText: GetLineText) : CodeFix =
  Run.ifDiagnosticByCode (Set.ofList [ "748"; "747" ]) (fun diagnostic codeActionParams ->
    asyncResult {
      let fileName = codeActionParams.TextDocument.GetFilePath() |> Utils.normalizePath

      let fcsPos = protocolPosToPos diagnostic.Range.Start
      let! (tyRes, line, lines) = getParseResultsForFile fileName fcsPos
      let fcsErrorPos = protocolPosToPos diagnostic.Range.Start

      match tyRes.GetParseResults.TryRangeOfExprInYieldOrReturn fcsErrorPos with
      | None -> return []
      | Some exprRange ->
        let protocolExprRange = fcsRangeToLsp exprRange
        let! exprText = getLineText lines protocolExprRange
        let! errorText = getLineText lines diagnostic.Range

        let! title =
          if errorText.StartsWith("return!", StringComparison.Ordinal) then
            Ok(title "return!")
          elif errorText.StartsWith("yield!", StringComparison.Ordinal) then
            Ok(title "yield!")
          elif errorText.StartsWith("return", StringComparison.Ordinal) then
            Ok(title "return")
          elif errorText.StartsWith("yield", StringComparison.Ordinal) then
            Ok(title "yield")
          else
            Error "unknown start token for remove return or yield codefix"

        return
          [ { Title = title
              File = codeActionParams.TextDocument
              SourceDiagnostic = Some diagnostic
              Edits =
                [| { Range = diagnostic.Range
                     NewText = exprText } |]
              Kind = FixKind.Refactor } ]

    })
