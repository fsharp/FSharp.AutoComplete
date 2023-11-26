module FsAutoComplete.NestedLanguages

open FsAutoComplete.Logging
open FsToolkit.ErrorHandling
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Symbols

#nowarn "57" // from-end slicing

let logger = LogProvider.getLoggerByName "NestedLanguages"

type private StringParameter =
  { methodIdent: LongIdent
    parameterRange: Range
    rangesToRemove: Range[]
    parameterPosition: int }

let discoverRangesToRemoveForInterpolatedString (list: SynInterpolatedStringPart list) =
  list
  |> List.choose (function
    | SynInterpolatedStringPart.FillExpr(fillExpr = e) -> Some e.Range
    | _ -> None)
  |> List.toArray

let private (|Ident|_|) (e: SynExpr) =
  match e with
  | SynExpr.LongIdent(longDotId = SynLongIdent(id = ident)) -> Some ident
  | _ -> None

let rec private (|IsApplicationWithStringParameters|_|) (e: SynExpr) : option<StringParameter[]> =
  match e with
  // lines inside a binding
  // let doThing () =
  //    c.M("<div>")
  //    c.M($"<div>{1 + 1}")
  //    "<div>" |> c.M
  //    $"<div>{1 + 1}" |> c.M
  | SynExpr.Sequential(expr1 = e1; expr2 = e2) ->
    [| match e1 with
       | IsApplicationWithStringParameters(stringParameter) -> yield! stringParameter
       | _ -> ()

       match e2 with
       | IsApplicationWithStringParameters(stringParameter) -> yield! stringParameter
       | _ -> () |]
    // TODO: check if the array would be empty and return none
    |> Some

  // method call with string parameter - c.M("<div>")
  | SynExpr.App(
      funcExpr = Ident(ident); argExpr = SynExpr.Paren(expr = SynExpr.Const(SynConst.String(_text, _kind, range), _)))
  // method call with string parameter - c.M "<div>"
  | SynExpr.App(funcExpr = Ident(ident); argExpr = SynExpr.Const(SynConst.String(_text, _kind, range), _)) ->
    Some(
      [| { methodIdent = ident
           parameterRange = range
           rangesToRemove = [||]
           parameterPosition = 0 } |]
    )
  // method call with interpolated string parameter - c.M $"<div>{1 + 1}"
  | SynExpr.App(
      funcExpr = SynExpr.LongIdent(longDotId = SynLongIdent(id = ident))
      argExpr = SynExpr.Paren(expr = SynExpr.InterpolatedString(contents = parts; range = range)))
  // method call with interpolated string parameter - c.M($"<div>{1 + 1}")
  | SynExpr.App(
    funcExpr = SynExpr.LongIdent(longDotId = SynLongIdent(id = ident))
    argExpr = SynExpr.InterpolatedString(contents = parts; range = range)) ->
    let rangesToRemove = discoverRangesToRemoveForInterpolatedString parts

    Some(
      [| { methodIdent = ident
           parameterRange = range
           rangesToRemove = rangesToRemove
           parameterPosition = 0 } |]
    )
  // piped method call with string parameter - "<div>" |> c.M
  // piped method call with interpolated parameter - $"<div>{1 + 1}" |> c.M
  // method call with multiple string or interpolated string parameters (this also covers the case when not all parameters of the member are strings)
  //   c.M("<div>", true) and/or c.M(true, "<div>")
  // piped method call with multiple string or interpolated string parameters (this also covers the case when not all parameters of the member are strings)
  // let binding that is a string value that has the stringsyntax attribute on it - [<StringSyntax("html")>] let html = "<div />"
  // all of the above but with literals
  | _ -> None

/// <summary></summary>
type private StringParameterFinder() =
  inherit SyntaxCollectorBase()

  let languages = ResizeArray<StringParameter>()

  override _.WalkBinding(SynBinding(expr = expr)) =
    match expr with
    | IsApplicationWithStringParameters(stringParameters) -> languages.AddRange stringParameters
    | _ -> ()

  override _.WalkSynModuleDecl(decl) =
    match decl with
    | SynModuleDecl.Expr(expr = IsApplicationWithStringParameters(stringParameters)) ->
      languages.AddRange stringParameters
    | _ -> ()

  member _.NestedLanguages = languages.ToArray()


let private findParametersForParseTree (p: ParsedInput) =
  let walker = StringParameterFinder()
  walkAst walker p
  walker.NestedLanguages

let private (|IsStringSyntax|_|) (a: FSharpAttribute) =
  match a.AttributeType.FullName with
  | "System.Diagnostics.CodeAnalysis.StringSyntaxAttribute" ->
    match a.ConstructorArguments |> Seq.tryHead with
    | Some(_ty, languageValue) -> Some(languageValue :?> string)
    | _ -> None
  | _ -> None

type NestedLanguageDocument = { Language: string; Ranges: Range[] }

let rangeMinusRanges (totalRange: Range) (rangesToRemove: Range[]) : Range[] =
  match rangesToRemove with
  | [||] -> [| totalRange |]
  | _ ->
    let mutable returnVal = ResizeArray()
    let mutable currentStart = totalRange.Start

    for r in rangesToRemove do
      returnVal.Add(Range.mkRange totalRange.FileName currentStart r.Start)
      currentStart <- r.End

    returnVal.Add(Range.mkRange totalRange.FileName currentStart totalRange.End)
    returnVal.ToArray()

let private parametersThatAreStringSyntax
  (
    parameters: StringParameter[],
    checkResults: FSharpCheckFileResults,
    text: IFSACSourceText
  ) : Async<NestedLanguageDocument[]> =
  async {
    let returnVal = ResizeArray()

    for p in parameters do
      let precedingParts, lastPart = p.methodIdent.[0..^1], p.methodIdent[^0]
      let endOfFinalTextToken = lastPart.idRange.End

      match text.GetLine(endOfFinalTextToken) with
      | None -> ()
      | Some lineText ->

        match
          checkResults.GetSymbolUseAtLocation(
            endOfFinalTextToken.Line,
            endOfFinalTextToken.Column,
            lineText,
            precedingParts |> List.map (fun i -> i.idText)
          )
        with
        | None -> ()
        | Some usage ->

          let sym = usage.Symbol
          // todo: keep MRU map of symbols to parameters and MRU of parameters to stringsyntax status

          match sym with
          | :? FSharpMemberOrFunctionOrValue as mfv ->
            let allParameters = mfv.CurriedParameterGroups |> Seq.collect id |> Seq.toArray
            let fsharpP = allParameters[p.parameterPosition]

            match fsharpP.Attributes |> Seq.tryPick (|IsStringSyntax|_|) with
            | Some language ->
              returnVal.Add
                { Language = language
                  Ranges = rangeMinusRanges p.parameterRange p.rangesToRemove }
            | None -> ()
          | _ -> ()

    return returnVal.ToArray()
  }

/// to find all of the nested language highlights, we're going to do the following:
/// * find all of the interpolated strings or string literals in the file that are in parameter-application positions
/// * get the method calls happening at those positions to check if that method has the StringSyntaxAttribute
/// * if so, return a) the language in the StringSyntaxAttribute, and b) the range of the interpolated string
let findNestedLanguages (tyRes: ParseAndCheckResults, text: VolatileFile) : NestedLanguageDocument[] Async =
  async {
    // get all string constants
    let potentialParameters = findParametersForParseTree tyRes.GetAST

    logger.info (
      Log.setMessageI
        $"Found {potentialParameters.Length:stringParams} potential parameters in {text.FileName:filename}@{text.Version:version}"
    )

    for p in potentialParameters do
      logger.info (
        Log.setMessageI
          $"Potential parameter: {p.parameterRange:range} in member {p.methodIdent:methodName} of {text.FileName:filename}@{text.Version:version} -> {text.Source[p.parameterRange]:sourceText}"
      )

    let! actualStringSyntaxParameters =
      parametersThatAreStringSyntax (potentialParameters, tyRes.GetCheckResults, text.Source)

    logger.info (
      Log.setMessageI
        $"Found {actualStringSyntaxParameters.Length:stringParams} actual parameters in {text.FileName:filename}@{text.Version:version}"
    )

    return actualStringSyntaxParameters
  }