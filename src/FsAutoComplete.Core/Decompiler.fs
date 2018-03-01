﻿module FsAutoComplete.Decompiler

open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices
open Utils
open System.Text.RegularExpressions
open Mono.Cecil
open ICSharpCode.Decompiler.CSharp
open ICSharpCode.Decompiler.CSharp.OutputVisitor
open ICSharpCode.Decompiler
open ICSharpCode.Decompiler.TypeSystem
open ICSharpCode.Decompiler.CSharp.Syntax
open ICSharpCode.Decompiler.CSharp.Transforms
open System.Collections.Generic

type TextWriterWithLocationFinder(tokenWriter:TextWriterTokenWriter, formattingPolicy, symbol:ISymbol option) =
    inherit CSharpOutputVisitor(tokenWriter, formattingPolicy)

    let tokenWriter = tokenWriter

    let mutable matchingLocation = None

    let saveMatchingSymbol (node:#AstNode) =
        let result = node.GetResolveResult()
        match symbol with
        | Some s when result.GetSymbol() = s -> 
            matchingLocation <- Some tokenWriter.Location
        | _ -> ()

    override this.VisitConstructorDeclaration(declaration:ConstructorDeclaration) =
        saveMatchingSymbol declaration
        base.VisitConstructorDeclaration(declaration)
        
    override this.VisitMethodDeclaration(declaration:MethodDeclaration) =
        saveMatchingSymbol declaration
        base.VisitMethodDeclaration(declaration)

    override this.VisitPropertyDeclaration(declaration:PropertyDeclaration) =
        saveMatchingSymbol declaration
        base.VisitPropertyDeclaration(declaration)

    override this.VisitFieldDeclaration(declaration:FieldDeclaration) =
        saveMatchingSymbol declaration
        base.VisitFieldDeclaration(declaration)

    override this.VisitEventDeclaration(declaration:EventDeclaration) =
        saveMatchingSymbol declaration
        base.VisitEventDeclaration(declaration)

    override this.VisitTypeDeclaration(declaration:TypeDeclaration) =
        saveMatchingSymbol declaration
        base.VisitTypeDeclaration(declaration)

    member __.MatchingLocation with get() = matchingLocation

let decompileTypeAndFindLocation (typeSystem:DecompilerTypeSystem) (typeDefinition:ITypeDefinition) (symbol:ISymbol option) =
    let sw = new StringWriter()
    let writerVisitor = 
        TextWriterWithLocationFinder(TextWriterTokenWriter(sw), FormattingOptionsFactory.CreateSharpDevelop(), symbol)

    let decompiler = 
        new CSharpDecompiler(typeSystem, new DecompilerSettings())
        
    decompiler.AstTransforms.Add(new EscapeInvalidIdentifiers())
    
    let syntaxTree = 
        typeDefinition.FullTypeName
        |> decompiler.DecompileType

    syntaxTree.AcceptVisitor(writerVisitor)
    sw.GetStringBuilder() |> string, writerVisitor.MatchingLocation

let loadTypeSystem (assemblyPath:string) =
    UniversalAssemblyResolver.LoadMainModule(assemblyPath,false, true)
    |> DecompilerTypeSystem

let resolveType (typeSystem:DecompilerTypeSystem) (typeName:string) =
    typeName
    |> FullTypeName
    |> typeSystem.MainAssembly.GetTypeDefinition

let rec formatExtTypeFullName externalType =
    match externalType with
    | ExternalType.Type (name, genericArgs) ->
        match genericArgs with
        | [] -> ""
        | args ->
            args
            |> List.map (formatExtTypeFullName >> sprintf "[%s]")
            |> String.concat ","
            |> sprintf "[%s]"
        |> sprintf "%s%s" name
    | ExternalType.Array inner -> sprintf "%s[]" (formatExtTypeFullName inner)
    | ExternalType.Pointer inner -> sprintf "&%s" (formatExtTypeFullName inner)
    | ExternalType.TypeVar name -> sprintf "%s" name

let areSameTypes (typeArguments:IList<IType>) ((mParam,paramSym):IParameter*ParamTypeSymbol) =
    let compareToExternalType (extType:ExternalType) =
        let parameterTypeFullName = 
            typeArguments
            |> Seq.fold 
                (fun (str:string) t -> str.Replace(t.ReflectionName, t.Name) ) 
                (mParam.Type.ReflectionName.Trim([| '&'; ' '|]))

        let extTypeFullName = formatExtTypeFullName extType
        parameterTypeFullName = extTypeFullName

    match paramSym with
    | ParamTypeSymbol.Param extType -> compareToExternalType extType
    | ParamTypeSymbol.Byref extType ->
        mParam.IsRef && compareToExternalType extType


let getDeclaringTypeName = function
    | ExternalSymbol.Type (fullName) -> fullName
    | ExternalSymbol.Constructor (typeName, _args) -> typeName
    | ExternalSymbol.Method (typeName, _name, _paramSyms, _genericArity) -> typeName
    | ExternalSymbol.Field (typeName, _name) -> typeName
    | ExternalSymbol.Event (typeName, _name) -> typeName
    | ExternalSymbol.Property (typeName, _name) -> typeName

let findMethodFromArgs (args:ParamTypeSymbol list) (methods:IMethod seq) =
    methods
    |> Seq.tryFind (fun m ->
        let mParams = m.Parameters

        mParams.Count = args.Length
        && (Seq.zip mParams args) |> Seq.forall (areSameTypes m.TypeArguments) )

type ExternalContentPosition =
  { File: string
    Column: int
    Line: int }

let toSafeFileNameRegex = 
    System.Text.RegularExpressions.Regex("[^\w\.`\s]+", RegexOptions.Compiled)

let toSafeFileName (typeDef:ITypeDefinition) =
    let str = sprintf "%s %s.cs" typeDef.FullName typeDef.ParentAssembly.FullAssemblyName
    toSafeFileNameRegex.Replace(str, "_")

let decompile (externalSym:ExternalSymbol) assemblyPath =
    try
        let typeSystem = 
            loadTypeSystem assemblyPath

        let typeDef =
            getDeclaringTypeName externalSym
            |> resolveType typeSystem

        let symbol =
            match externalSym with
            | ExternalSymbol.Type _ -> Some (typeDef :> ISymbol)
            | ExternalSymbol.Constructor (_typeName, args) ->
                typeDef.GetConstructors()
                |> findMethodFromArgs args
                |> Option.map (fun x -> x :> ISymbol)
            
            | ExternalSymbol.Method (_typeName, name, args, genericArity) ->
                typeDef.GetMethods(filter = Predicate (fun m -> m.Name = name))
                |> Seq.where (fun m -> m.TypeParameters.Count = genericArity)
                |> findMethodFromArgs args
                |> Option.map (fun x -> x :> ISymbol)
            
            | ExternalSymbol.Field (_typeName, name) ->
                typeDef.GetFields(filter = Predicate (fun m -> m.Name = name))
                |> Seq.tryHead
                |> Option.map (fun x -> x :> ISymbol)
            
            | ExternalSymbol.Event (_typeName, name) ->
                typeDef.GetEvents(filter = Predicate (fun m -> m.Name = name))
                |> Seq.tryHead
                |> Option.map (fun x -> x :> ISymbol)
            
            | ExternalSymbol.Property (_typeName, name) ->
                typeDef.GetProperties(filter = Predicate (fun m -> m.Name = name))
                |> Seq.tryHead
                |> Option.map (fun x -> x :> ISymbol)
        
    
        let (contents, location) = 
            decompileTypeAndFindLocation typeSystem typeDef symbol

        let fileName = typeDef |> toSafeFileName
        let tempFile =
            System.IO.Path.GetTempPath() </> fileName

        System.IO.File.WriteAllText(tempFile, contents)
    
        match location with
        | Some l ->
            Some { File = tempFile
                   Column = l.Column - 1
                   Line = l.Line }
        | None ->
            Some { File = tempFile
                   Column = 1
                   Line = 1 }
    with
    | _ -> None

let tryFindExternalDeclaration (checkResults:FSharpCheckFileResults) (assembly, externalSym) =
    checkResults.ProjectContext.GetReferencedAssemblies()
    |> List.tryFind(fun x -> x.SimpleName = assembly)
    |> Option.bind(fun x -> x.FileName)
    |> Option.bind(decompile externalSym)
