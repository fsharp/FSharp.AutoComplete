namespace FsAutoComplete

open System
open System.IO

open Microsoft.FSharp.Compiler.SourceCodeServices

module ProjectCrackerProjectJson =

  let getProjectOptionsFromResponseFile (file : string)  =
    let projDir = Path.GetDirectoryName file
    let rsp =
      Directory.GetFiles(projDir, "dotnet-compile-fsc.rsp", SearchOption.AllDirectories)
      |> Seq.head
      |> File.ReadAllLines
      |> Array.map (fun s -> if s.EndsWith ".fs" then
                                let p = Path.GetFullPath s
                                (p.Chars 0).ToString().ToLower() + p.Substring(1)
                             else s )
      |> Array.filter((<>) "--nocopyfsharpcore")

    let outType = FscArguments.outType (rsp |> List.ofArray)

    {
      ProjectFileName = file
      SourceFiles = [||]
      OtherOptions = rsp
      ReferencedProjects = [||]
      IsIncompleteTypeCheckEnvironment = false
      UseScriptResolutionRules = false
      LoadTime = DateTime.Now
      UnresolvedReferences = None;
      Stamp = None
      OriginalLoadReferences = []
      ExtraProjectInfo = Some (box {
        ExtraProjectInfoData.ProjectSdkType = ProjectSdkType.ProjectJson
        ExtraProjectInfoData.ProjectOutputType = outType
      })
    }

  let load file =
    if not (File.Exists file) then
      Err (GenericError(sprintf "File '%s' does not exist" file))
    else
      try
        let po = getProjectOptionsFromResponseFile file
        let compileFiles = Seq.filter (fun (s:string) -> s.EndsWith(".fs")) po.OtherOptions
        let outputFile = FscArguments.outputFile (Path.GetDirectoryName(file)) (po.OtherOptions |> List.ofArray)
        let references = FscArguments.references (po.OtherOptions |> List.ofArray)
        Ok (po, Seq.toList compileFiles, outputFile, Seq.toList references, Map<string,string>([||]))
      with e ->
        Err (GenericError(e.Message))
