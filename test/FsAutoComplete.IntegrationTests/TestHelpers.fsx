open System
open System.IO
open System.Diagnostics
open System.Text.RegularExpressions

#I "../../packages/Newtonsoft.Json/lib/net45/"
#r "Newtonsoft.Json.dll"
open Newtonsoft.Json

let (</>) a b = Path.Combine(a,b)

type FsAutoCompleteWrapper() =

  let p = new System.Diagnostics.Process()
  let cachedOutput = new Text.StringBuilder()

  do
    p.StartInfo.FileName <-
      IO.Path.Combine(__SOURCE_DIRECTORY__,
                      "../../src/FsAutoComplete/bin/Debug/fsautocomplete.exe")
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

  member x.completion (fn: string) (lineStr:string)(line: int) (col: int) : unit =
    fprintf p.StandardInput "completion \"%s\" \"%s\" %d %d\n" fn lineStr line col

  member x.methods (fn: string) (lineStr: string)(line: int) (col: int) : unit =
    fprintf p.StandardInput "methods \"%s\" \"%s\" %d %d\n" fn lineStr line col

  member x.completionFilter (fn: string) (lineStr: string)(line: int) (col: int) (filter: string) : unit =
    fprintf p.StandardInput "completion \"%s\" \"%s\" %d %d filter=%s\n" fn lineStr line col filter

  member x.tooltip (fn: string) (lineStr: string) (line: int) (col: int) : unit =
    fprintf p.StandardInput "tooltip \"%s\" \"%s\" %d %d\n" fn lineStr line col

  member x.typesig (fn: string) (lineStr: string) (line: int) (col: int) : unit =
    fprintf p.StandardInput "typesig \"%s\" \"%s\" %d %d\n" fn lineStr line col

  member x.finddeclaration (fn: string) (lineStr: string) (line: int) (col: int) : unit =
    fprintf p.StandardInput "finddecl \"%s\" \"%s\" %d %d\n" fn lineStr line col

  member x.symboluse (fn: string) (lineStr: string) (line: int) (col: int) : unit =
    fprintf p.StandardInput "symboluse \"%s\" \"%s\" %d %d\n" fn lineStr line col

  member x.declarations (fn: string) : unit =
    fprintf p.StandardInput "declarations \"%s\"\n" fn

  member x.lint (fn: string) : unit =
    fprintf p.StandardInput "lint \"%s\"\n" fn

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
                                 "/.*?test/FsAutoComplete\.IntegrationTests/(.*?(\"|$))",
                                 "<absolute path removed>/FsAutoComplete.IntegrationTests/$1")
      lines.[i] <- Regex.Replace(lines.[i],
                                 "\"/[^\"]*?/([^\"/]*?\.dll\")",
                                  "\"<absolute path removed>/$1")
    else
      if Path.GetExtension fn = ".json" then
        lines.[i] <- Regex.Replace(lines.[i].Replace(@"\\", "/"),
                                   "[a-zA-Z]:/.*?test/FsAutoComplete\.IntegrationTests/(.*?(\"|$))",
                                   "<absolute path removed>/FsAutoComplete.IntegrationTests/$1")
        lines.[i] <- Regex.Replace(lines.[i],
                                   "\"[a-zA-Z]:/[^\"]*?/([^\"/]*?\.dll\")",
                                   "\"<absolute path removed>/$1")
      else
        lines.[i] <- Regex.Replace(lines.[i].Replace('\\','/'),
                                   "[a-zA-Z]:/.*?test/FsAutoComplete\.IntegrationTests/(.*?(\"|$))",
                                   "<absolute path removed>/FsAutoComplete.IntegrationTests/$1")


    lines.[i] <- lines.[i].Replace("\r", "").Replace(@"\r", "")

  // Write manually to ensure \n line endings on all platforms
  using (new StreamWriter(fn))
  <| fun f ->
      for line in lines do
        f.Write(line)
        f.Write('\n')

let runProcess (workingDir: string) (exePath: string) (args: string) =
    let psi = System.Diagnostics.ProcessStartInfo()
    psi.FileName <- exePath
    psi.WorkingDirectory <- workingDir 
    psi.RedirectStandardOutput <- false
    psi.RedirectStandardError <- false
    psi.Arguments <- args
    psi.CreateNoWindow <- true
    psi.UseShellExecute <- false

    use p = new System.Diagnostics.Process()
    p.StartInfo <- psi
    p.Start() |> ignore
    p.WaitForExit()
      
    let exitCode = p.ExitCode
    exitCode
