open System
open System.IO
open System.Diagnostics
open System.Text.RegularExpressions

#I "../../../packages/Newtonsoft.Json/lib/net45/"
#r "Newtonsoft.Json.dll"
#I "../../../packages/Fantomas/lib/"
#r "FantomasLib.dll"

open Newtonsoft.Json
open Fantomas.FormatConfig

type FsAutoCompleteWrapper() =

  let p = new System.Diagnostics.Process()
  let cachedOutput = new Text.StringBuilder()
  let sendconfig (proc : System.Diagnostics.Process) (config : FormatConfig) = 
    let p f = fprintf proc.StandardInput f
    p " config "
    p "spaceindent %d " config.IndentSpaceNum
    p "pagewidth %d " config.PageWidth
    p "endsemicolon %b " config.SemicolonAtEndOfLine
    p "spacebeforearg %b " config.SpaceBeforeArgument
    p "spacebeforecolon %b " config.SpaceBeforeColon
    p "spaceaftersemi %b " config.SpaceAfterSemicolon
    p "indenttrywith %b " config.IndentOnTryWith
    p "reorderopens %b " config.ReorderOpenDeclaration
    p "surrounddelims %b " config.SpaceAroundDelimiter
    p "strict %b" config.StrictMode // NOTE we don't send spaces here after config, because that blows the parser
    
  do
    p.StartInfo.FileName <-
      IO.Path.Combine(__SOURCE_DIRECTORY__,
                      "../../bin/Debug/fsautocomplete.exe")
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.RedirectStandardError  <- true
    p.StartInfo.RedirectStandardInput  <- true
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.EnvironmentVariables.Add("FCS_ToolTipSpinWaitTime", "10000")
    p.Start () |> ignore

  member x.project (s: string) : unit =
    fprintf p.StandardInput "project \"%s\"\n" s

  member x.parse (s: string) : unit =
    let text = if IO.File.Exists s then IO.File.ReadAllText(s) else ""
    fprintf p.StandardInput "parse \"%s\" sync\n%s\n<<EOF>>\n" s text

  member x.parseContent (filename: string) (content: string) : unit =
    fprintf p.StandardInput "parse \"%s\" sync\n%s\n<<EOF>>\n" filename content

  member x.completion (fn: string) (line: int) (col: int) : unit =
    fprintf p.StandardInput "completion \"%s\" %d %d\n" fn line col

  member x.methods (fn: string) (line: int) (col: int) : unit =
    fprintf p.StandardInput "methods \"%s\" %d %d\n" fn line col

  member x.completionFilter (fn: string) (line: int) (col: int) (filter: string) : unit =
    fprintf p.StandardInput "completion \"%s\" %d %d filter=%s\n" fn line col filter

  member x.tooltip (fn: string) (line: int) (col: int) : unit =
    fprintf p.StandardInput "tooltip \"%s\" %d %d\n" fn line col

  member x.finddeclaration (fn: string) (line: int) (col: int) : unit =
    fprintf p.StandardInput "finddecl \"%s\" %d %d\n" fn line col

  member x.symboluse (fn: string) (line: int) (col: int) : unit =
    fprintf p.StandardInput "symboluse \"%s\" %d %d\n" fn line col

  member x.declarations (fn: string) : unit =
    fprintf p.StandardInput "declarations \"%s\"\n" fn

  member x.lint (fn: string) : unit =
    fprintf p.StandardInput "lint \"%s\"\n" fn

  member x.format (fn : string) (config : FormatConfig option) : unit = 
    fprintf p.StandardInput "format \"%s\"" fn
    config |> Option.map (sendconfig p) |> ignore
    fprintf p.StandardInput "\n"
  
  member x.formatselection (fn : string) (selection : int*int*int*int) (config : FormatConfig option) : unit =
    fprintf p.StandardInput "format \"%s\"" fn
    let (sl,sc,el,ec) = selection
    fprintf p.StandardInput " range %d:%d-%d:%d" sl sc el ec
    config |> Option.map (sendconfig p) |> ignore
    fprintf p.StandardInput "\n"
    
  member x.send (s: string) : unit =
    fprintf p.StandardInput "%s" s

  /// Wait for a single line to be output (one JSON message)
  /// Note that this line will appear at the *start* of output.json,
  /// so use carefully, and preferably only at the beginning.
  member x.waitForLine () : unit =
    cachedOutput.AppendLine(p.StandardOutput.ReadLine()) |> ignore

  member x.finalOutput () : string =
    let s = p.StandardOutput.ReadToEnd()
    let t = p.StandardError.ReadToEnd()
    p.WaitForExit()
    cachedOutput.ToString() + s + t

let formatJson json =
    try
      let parsedJson = JsonConvert.DeserializeObject(json)
      JsonConvert.SerializeObject(parsedJson, Formatting.Indented)
    with _ -> json

let writeNormalizedOutput (fn: string) (s: string) =
  let lines = s.TrimEnd().Split('\n')
  for i in [ 0 .. lines.Length - 1 ] do
    if Path.GetExtension fn = ".json" then
      lines.[i] <- formatJson lines.[i]

    if Path.DirectorySeparatorChar = '/' then
      lines.[i] <- Regex.Replace(lines.[i],
                                 "/.*?FsAutoComplete/test/(.*?(\"|$))",
                                 "<absolute path removed>/test/$1")
      lines.[i] <- Regex.Replace(lines.[i],
                                 "\"/[^\"]*?/([^\"/]*?\.dll\")",
                                  "\"<absolute path removed>/$1")
    else
      if Path.GetExtension fn = ".json" then
        lines.[i] <- Regex.Replace(lines.[i].Replace(@"\\", "/"),
                                   "[a-zA-Z]:/.*?FsAutoComplete/test/(.*?(\"|$))",
                                   "<absolute path removed>/test/$1")
        lines.[i] <- Regex.Replace(lines.[i],
                                   "\"[a-zA-Z]:/[^\"]*?/([^\"/]*?\.dll\")",
                                   "\"<absolute path removed>/$1")
      else
        lines.[i] <- Regex.Replace(lines.[i].Replace('\\','/'),
                                   "[a-zA-Z]:/.*?FsAutoComplete/test/(.*?(\"|$))",
                                   "<absolute path removed>/test/$1")


    lines.[i] <- lines.[i].Replace("\r", "")

  // Write manually to ensure \n line endings on all platforms
  using (new StreamWriter(fn))
  <| fun f ->
      for line in lines do
        f.Write(line)
        f.Write('\n')
