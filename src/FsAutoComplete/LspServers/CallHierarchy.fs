namespace FsAutoComplete.Lsp


module CallHierarchy =
  open FSharp.Compiler.Symbols

  let gatherFSharpMemberOrFunction memberCallHandler (fileDecls : FSharpImplementationFileDeclaration seq) =

      let rec visitExpr memberCallHandler (e:FSharpExpr) =

          match e with
          | FSharpExprPatterns.AddressOf(lvalueExpr) ->
              visitExpr memberCallHandler lvalueExpr
          | FSharpExprPatterns.AddressSet(lvalueExpr, rvalueExpr) ->
              visitExpr memberCallHandler lvalueExpr; visitExpr memberCallHandler rvalueExpr
          | FSharpExprPatterns.Application(funcExpr, typeArgs, argExprs) ->
              visitExpr memberCallHandler funcExpr; visitExprs memberCallHandler argExprs
          | FSharpExprPatterns.Call(objExprOpt, memberOrFunc, typeArgs1, typeArgs2, argExprs) ->
              memberCallHandler (e.Range,memberOrFunc)
              visitObjArg memberCallHandler objExprOpt; visitExprs memberCallHandler argExprs
          | FSharpExprPatterns.Coerce(targetType, inpExpr) ->
              visitExpr memberCallHandler inpExpr
          | FSharpExprPatterns.FastIntegerForLoop(startExpr, limitExpr, consumeExpr, isUp, _, _) ->
              visitExpr memberCallHandler startExpr; visitExpr memberCallHandler limitExpr; visitExpr memberCallHandler consumeExpr
          | FSharpExprPatterns.ILAsm(asmCode, typeArgs, argExprs) ->
              visitExprs memberCallHandler argExprs
          | FSharpExprPatterns.ILFieldGet (objExprOpt, fieldType, fieldName) ->
              visitObjArg memberCallHandler objExprOpt
          | FSharpExprPatterns.ILFieldSet (objExprOpt, fieldType, fieldName, valueExpr) ->
              visitObjArg memberCallHandler objExprOpt
          | FSharpExprPatterns.IfThenElse (guardExpr, thenExpr, elseExpr) ->
              visitExpr memberCallHandler guardExpr; visitExpr memberCallHandler thenExpr; visitExpr memberCallHandler elseExpr
          | FSharpExprPatterns.Lambda(lambdaVar, bodyExpr) ->
              visitExpr memberCallHandler bodyExpr
          | FSharpExprPatterns.Let((bindingVar, bindingExpr, _), bodyExpr) ->
              visitExpr memberCallHandler bindingExpr; visitExpr memberCallHandler bodyExpr
          | FSharpExprPatterns.LetRec(recursiveBindings, bodyExpr) ->
              List.iter ((fun (_, x, _) -> x)  >> visitExpr memberCallHandler) recursiveBindings; visitExpr memberCallHandler bodyExpr
          | FSharpExprPatterns.NewArray(arrayType, argExprs) ->
              visitExprs memberCallHandler argExprs
          | FSharpExprPatterns.NewDelegate(delegateType, delegateBodyExpr) ->
              visitExpr memberCallHandler delegateBodyExpr
          | FSharpExprPatterns.NewObject(objType, typeArgs, argExprs) ->
              visitExprs memberCallHandler argExprs
          | FSharpExprPatterns.NewRecord(recordType, argExprs) ->
              visitExprs memberCallHandler argExprs
          | FSharpExprPatterns.NewTuple(tupleType, argExprs) ->
              visitExprs memberCallHandler argExprs
          | FSharpExprPatterns.NewUnionCase(unionType, unionCase, argExprs) ->
              visitExprs memberCallHandler argExprs
          | FSharpExprPatterns.Quote(quotedExpr) ->
              visitExpr memberCallHandler quotedExpr
          | FSharpExprPatterns.FSharpFieldGet(objExprOpt, recordOrClassType, fieldInfo) ->
              visitObjArg memberCallHandler objExprOpt
          | FSharpExprPatterns.FSharpFieldSet(objExprOpt, recordOrClassType, fieldInfo, argExpr) ->
              visitObjArg memberCallHandler objExprOpt; visitExpr memberCallHandler argExpr
          | FSharpExprPatterns.Sequential(firstExpr, secondExpr) ->
              visitExpr memberCallHandler firstExpr; visitExpr memberCallHandler secondExpr
          | FSharpExprPatterns.TryFinally(bodyExpr, finalizeExpr, _, _) ->
              visitExpr memberCallHandler bodyExpr; visitExpr memberCallHandler finalizeExpr
          | FSharpExprPatterns.TryWith(bodyExpr, _, _, catchVar, catchExpr, _, _) ->
              visitExpr memberCallHandler bodyExpr; visitExpr memberCallHandler catchExpr
          | FSharpExprPatterns.TupleGet(tupleType, tupleElemIndex, tupleExpr) ->
              visitExpr memberCallHandler tupleExpr
          | FSharpExprPatterns.DecisionTree(decisionExpr, decisionTargets) ->
              visitExpr memberCallHandler decisionExpr; List.iter (snd >> visitExpr memberCallHandler) decisionTargets
          | FSharpExprPatterns.DecisionTreeSuccess (decisionTargetIdx, decisionTargetExprs) ->
              visitExprs memberCallHandler decisionTargetExprs
          | FSharpExprPatterns.TypeLambda(genericParam, bodyExpr) ->
              visitExpr memberCallHandler bodyExpr
          | FSharpExprPatterns.TypeTest(ty, inpExpr) ->
              visitExpr memberCallHandler inpExpr
          | FSharpExprPatterns.UnionCaseSet(unionExpr, unionType, unionCase, unionCaseField, valueExpr) ->
              visitExpr memberCallHandler unionExpr; visitExpr memberCallHandler valueExpr
          | FSharpExprPatterns.UnionCaseGet(unionExpr, unionType, unionCase, unionCaseField) ->
              visitExpr memberCallHandler unionExpr
          | FSharpExprPatterns.UnionCaseTest(unionExpr, unionType, unionCase) ->
              visitExpr memberCallHandler unionExpr
          | FSharpExprPatterns.UnionCaseTag(unionExpr, unionType) ->
              visitExpr memberCallHandler unionExpr
          | FSharpExprPatterns.ObjectExpr(objType, baseCallExpr, overrides, interfaceImplementations) ->
              visitExpr memberCallHandler baseCallExpr
              List.iter (visitObjMember memberCallHandler) overrides
              List.iter (snd >> List.iter (visitObjMember memberCallHandler)) interfaceImplementations
          | FSharpExprPatterns.TraitCall(sourceTypes, traitName, typeArgs, typeInstantiation, argTypes, argExprs) ->
              visitExprs memberCallHandler argExprs
          | FSharpExprPatterns.ValueSet(valToSet, valueExpr) ->
              visitExpr memberCallHandler valueExpr
          | FSharpExprPatterns.WhileLoop(guardExpr, bodyExpr, _) ->
              visitExpr memberCallHandler guardExpr; visitExpr memberCallHandler bodyExpr
          | FSharpExprPatterns.BaseValue baseType -> ()
          | FSharpExprPatterns.DefaultValue defaultType -> ()
          | FSharpExprPatterns.ThisValue thisType -> ()
          | FSharpExprPatterns.Const(constValueObj, constType) -> ()
          | FSharpExprPatterns.Value(valueToGet) -> ()
          | _ -> ()

      and visitExprs f exprs =
          List.iter (visitExpr f) exprs

      and visitObjArg f objOpt =
          Option.iter (visitExpr f) objOpt

      and visitObjMember f memb =
          visitExpr f memb.Body

      let rec visitDeclaration f d =
          match d with
          | FSharpImplementationFileDeclaration.Entity (e, subDecls) ->
              for subDecl in subDecls do
                  visitDeclaration f subDecl
          | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue(v, vs, e) ->
              visitExpr f e
          | FSharpImplementationFileDeclaration.InitAction(e) ->
              visitExpr f e


      fileDecls |> Seq.iter (visitDeclaration memberCallHandler)
