module FsAutoComplete.WorkspacePeek

open System
open System.IO

type SolutionData = {
    Items: SolutionItem list
    Configurations: SolutionConfiguration list
    }
and SolutionConfiguration = {
    Id: string
    ConfigurationName: string
    PlatformName: string
    IncludeInBuild: bool
    }
and SolutionItem = {
    Guid: Guid
    Name: string
    Kind: SolutionItemKind
    }
and SolutionItemKind =
    | MsbuildFormat of SolutionItemMsbuildConfiguration list
    | Folder of (SolutionItem list) * (string list)
    | Unsupported
    | Unknown
and SolutionItemMsbuildConfiguration = {
    Id: string
    ConfigurationName: string
    PlatformName: string
    }

[<RequireQualifiedAccess>]
type Interesting =
| Solution of string * SolutionData
| Directory of string * string list

let tryParseSln slnFilePath =
    let parseSln (sln: Microsoft.Build.Construction.SolutionFile) =
        let slnDir = Path.GetDirectoryName slnFilePath
        let makeAbsoluteFromSlnDir =
            let makeAbs path =
                if Path.IsPathRooted path then
                    path
                else
                    Path.Combine(slnDir, path)
                    |> Path.GetFullPath
            Utils.normalizeDirSeparators >> makeAbs
        let rec parseItem (item: Microsoft.Build.Construction.ProjectInSolution) =
            let parseKind (item: Microsoft.Build.Construction.ProjectInSolution) =
                match item.ProjectType with
                | Microsoft.Build.Construction.SolutionProjectType.KnownToBeMSBuildFormat ->
                    (item.RelativePath |> makeAbsoluteFromSlnDir), SolutionItemKind.MsbuildFormat []
                | Microsoft.Build.Construction.SolutionProjectType.SolutionFolder ->
                    let children =
                        sln.ProjectsInOrder
                        |> Seq.filter (fun x -> x.ParentProjectGuid = item.ProjectGuid)
                        |> Seq.map parseItem
                        |> List.ofSeq
                    let files =
                        item.FolderFiles
                        |> Seq.map makeAbsoluteFromSlnDir
                        |> List.ofSeq
                    item.ProjectName, SolutionItemKind.Folder (children, files)
                | Microsoft.Build.Construction.SolutionProjectType.EtpSubProject
                | Microsoft.Build.Construction.SolutionProjectType.WebDeploymentProject
                | Microsoft.Build.Construction.SolutionProjectType.WebProject ->
                    (item.ProjectName |> makeAbsoluteFromSlnDir), SolutionItemKind.Unsupported
                | Microsoft.Build.Construction.SolutionProjectType.Unknown
                | _ ->
                    (item.ProjectName |> makeAbsoluteFromSlnDir), SolutionItemKind.Unknown

            let name, itemKind = parseKind item
            { Guid = item.ProjectGuid |> Guid.Parse
              Name = name
              Kind = itemKind }

        let items =
            sln.ProjectsInOrder
            |> Seq.filter (fun x -> isNull x.ParentProjectGuid)
            |> Seq.map parseItem
        let data = {
            Items = items |> List.ofSeq
            Configurations = []
        }
        (slnFilePath, data)

    let slnFile =
        try
            Microsoft.Build.Construction.SolutionFile.Parse(slnFilePath)
            |> Some
        with _ ->
            None

    slnFile
    |> Option.map parseSln

open System.IO

type private UsefulFile =
    | FsProj
    | Sln
    | Fsx

let private partitionByChoice3 =
    let foldBy (a, b, c) t =
        match t with
        | Choice1Of3 x -> (x :: a, b, c)
        | Choice2Of3 x -> (a, x :: b, c)
        | Choice3Of3 x -> (a, b, x :: c)
    Array.fold foldBy ([],[],[])

let peek (rootDir: string) deep (excludedDirs: string list) =
    let dirInfo = DirectoryInfo(rootDir)

    //TODO accept glob list to ignore
    let ignored =
        let normalizedDirs = excludedDirs |> List.map (fun s -> s.ToUpperInvariant()) |> Array.ofList
        (fun (s: string) -> normalizedDirs |> Array.contains (s.ToUpperInvariant()))

    let scanDir (dirInfo: DirectoryInfo) =
        let hasExt (ext: string) (s: FileInfo) = s.FullName.EndsWith(ext)
        dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
        |> Seq.choose (fun s ->
            match s with
            | x when x |> hasExt ".sln" -> Some (UsefulFile.Sln, x)
            | x when x |> hasExt ".fsx" -> Some (UsefulFile.Fsx, x)
            | x when x |> hasExt ".fsproj" -> Some (UsefulFile.FsProj, x)
            | _ -> None)
        |> Seq.toArray

    let dirs =
        let rec scanDirs (dirInfo: DirectoryInfo) lvl =
            seq {
                if lvl <= deep then
                    yield dirInfo
                    for s in dirInfo.GetDirectories() do
                        if not(ignored s.Name) then
                            yield! scanDirs s (lvl + 1)
            }

        scanDirs dirInfo 0
        |> Array.ofSeq

    let getInfo (t, (f: FileInfo)) =
        match t with
        | UsefulFile.Sln ->
            tryParseSln f.FullName
            |> Option.map Choice1Of3
        | UsefulFile.Fsx ->
            Some (Choice2Of3 (f.FullName))
        | UsefulFile.FsProj ->
            Some (Choice3Of3 (f.FullName))

    let found =
        dirs
        |> Array.Parallel.collect scanDir
        |> Array.Parallel.choose getInfo

    let slns, _fsxs, fsprojs =
        found |> partitionByChoice3

    //TODO weight order of fsprojs from sln
    let dir = rootDir, (fsprojs |> List.sort)

    [ yield! slns |> List.map Interesting.Solution
      yield dir |> Interesting.Directory ]
