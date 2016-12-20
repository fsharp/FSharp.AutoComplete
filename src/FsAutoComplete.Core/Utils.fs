[<AutoOpen>]
module FsAutoComplete.Utils

open System.IO
open System.Collections.Concurrent
open System.Diagnostics
open System

type Result<'a> =
  | Success of 'a
  | Failure of string

type Pos =
    { Line: int
      Col: int }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Pos =
    let make line column = { Line = line; Col = column }


type Serializer = obj -> string
type ProjectFilePath = string
type SourceFilePath = string
type LineStr = string

let isAScript fileName =
    let ext = Path.GetExtension fileName
    [".fsx";".fsscript";".sketchfs"] |> List.exists ((=) ext)

let runningOnMono =
  try not << isNull <| Type.GetType "Mono.Runtime"
  with _ -> false

let normalizePath (file : string) =
  if file.EndsWith ".fs" then
      let p = Path.GetFullPath file
      (p.Chars 0).ToString().ToLower() + p.Substring(1)
  else file

let inline combinePaths path1 (path2 : string) = Path.Combine(path1, path2.TrimStart [| '\\'; '/' |])

let inline (</>) path1 path2 = combinePaths path1 path2

let private sepChar = Path.DirectorySeparatorChar

let normalizeDirSeparators (path: string) =
  match sepChar with
  | '\\' -> path.Replace('/', '\\')
  | '/' -> path.Replace('\\', '/')
  | _ -> path

[<RequireQualifiedAccess>]
module Option =

  let inline attempt (f: unit -> 'T) = try Some <| f() with _ -> None

  let getOrElse defaultValue option =
    match option with
    | None -> defaultValue
    | Some x -> x

  /// Gets the option if Some x, otherwise the supplied default value.
  let inline orElse v option =
    match option with
    | Some x -> Some x
    | None -> v


  let orElseFun other option =
    match option with
    | None -> other()
    | Some x -> Some x

  let getOrElseFun defaultValue option =
    match option with
    | None -> defaultValue()
    | Some x -> x

  let inline orTry f =
    function
    | Some x -> Some x
    | None -> f()

  /// Some(Some x) -> Some x | None -> None
  let inline flatten x =
    match x with
    | Some x -> x
    | None -> None


[<RequireQualifiedAccess>]
module Async =
    /// Transforms an Async value using the specified function.
    [<CompiledName("Map")>]
    let map (mapping : 'a -> 'b) (value : Async<'a>) : Async<'b> =
        async {
            // Get the input value.
            let! x = value
            // Apply the mapping function and return the result.
            return mapping x
        }

    // Transforms an Async value using the specified Async function.
    [<CompiledName("Bind")>]
    let bind (binding : 'a -> Async<'b>) (value : Async<'a>) : Async<'b> =
        async {
            // Get the input value.
            let! x = value
            // Apply the binding function and return the result.
            return! binding x
        }

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module Array =
    let inline private checkNonNull argName arg =
        match box arg with
        | null -> nullArg argName
        | _ -> ()

    /// Optimized arrays equality. ~100x faster than `array1 = array2` on strings.
    /// ~2x faster for floats
    /// ~0.8x slower for ints
    let inline areEqual (xs: 'T []) (ys: 'T []) =
        match xs, ys with
        | null, null -> true
        | [||], [||] -> true
        | null, _ | _, null -> false
        | _ when xs.Length <> ys.Length -> false
        | _ ->
            let mutable break' = false
            let mutable i = 0
            let mutable result = true
            while i < xs.Length && not break' do
                if xs.[i] <> ys.[i] then
                    break' <- true
                    result <- false
                i <- i + 1
            result


    /// Fold over the array passing the index and element at that index to a folding function
    let foldi (folder: 'State -> int -> 'T -> 'State) (state: 'State) (array: 'T []) =
        checkNonNull "array" array
        if array.Length = 0 then state else
        let folder = OptimizedClosures.FSharpFunc<_,_,_,_>.Adapt folder
        let mutable state:'State = state
        let len = array.Length
        for i = 0 to len - 1 do
            state <- folder.Invoke (state, i, array.[i])
        state

    /// Returns all heads of a given array.
    /// For [|1;2;3|] it returns [|[|1; 2; 3|]; [|1; 2|]; [|1|]|]
    let heads (array: 'T []) =
        checkNonNull "array" array
        let res = Array.zeroCreate<'T[]> array.Length
        for i = array.Length - 1 downto 0 do
            res.[i] <- array.[0..i]
        res

    /// check if subArray is found in the wholeArray starting
    /// at the provided index
    let inline isSubArray (subArray: 'T []) (wholeArray:'T []) index =
        if isNull subArray || isNull wholeArray then false
        elif subArray.Length = 0 then true
        elif subArray.Length > wholeArray.Length then false
        elif subArray.Length = wholeArray.Length then areEqual subArray wholeArray else
        let rec loop subidx idx =
            if subidx = subArray.Length then true
            elif subArray.[subidx] = wholeArray.[idx] then loop (subidx+1) (idx+1)
            else false
        loop 0 index

    /// Returns true if one array has another as its subset from index 0.
    let startsWith (prefix: _ []) (whole: _ []) =
        isSubArray prefix whole 0

    /// Returns true if one array has trailing elements equal to another's.
    let endsWith (suffix: _ []) (whole: _ []) =
        isSubArray suffix whole (whole.Length-suffix.Length)

    /// Returns a new array with an element replaced with a given value.
    let replace index value (array: _ []) =
        checkNonNull "array" array
        if index >= array.Length then raise (IndexOutOfRangeException "index")
        let res = Array.copy array
        res.[index] <- value
        res




[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module String =
    let (|StartsWith|_|) pattern value =
        if String.IsNullOrWhiteSpace value then
            None
        elif value.StartsWith pattern then
            Some()
        else None



type ConcurrentDictionary<'key, 'value> with
    member x.TryFind key =
        match x.TryGetValue key with
        | true, value -> Some value
        | _ -> None

    member x.ToSeq() =
        x |> Seq.map (fun (KeyValue(k, v)) -> k, v)

type Path with
    static member GetFullPathSafe path =
        try Path.GetFullPath path
        with _ -> path

    static member GetFileNameSafe path =
        try Path.GetFileName path
        with _ -> path



let inline debug msg = Printf.kprintf Debug.WriteLine msg
let inline fail msg = Printf.kprintf Debug.Fail msg