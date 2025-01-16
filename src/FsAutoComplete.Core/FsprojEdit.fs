namespace FsAutoComplete

open System.IO

module FsProjEditor =

  /// <summary>
  /// Check if the provided file is already included in the project via a <Compile Include="..." /> tag
  /// </summary>
  /// <param name="doc">Document to search in</param>
  /// <param name="searchedFile">File to search for</param>
  /// <returns>
  /// Returns true if the file is already included in the project via a <Compile Include="..." /> tag.
  ///
  /// Otherwise returns false.
  /// </returns>
  let private fileAlreadyIncludedViaCompileTag (doc: System.Xml.XmlDocument) (searchedFile: string) =

    let inline sanitizeFile (file: string) = file.Replace("\\", "/")

    let searchedFile = sanitizeFile searchedFile

    // Take all the <Compile Include="..." /> nodes
    doc.SelectNodes("//Compile[@Include]")
    |> Seq.cast<System.Xml.XmlNode>
    // Find the node that match the file we want to remove
    // Note: We sanitize the file name path because it can changes depending on the OS
    // and also on if the user used / or \ as the separator
    |> Seq.tryFind (fun node ->
      let sanitizedInclude = node.Attributes.["Include"].InnerText |> sanitizeFile

      sanitizedInclude = searchedFile)
    |> Option.isSome


  /// <summary>
  /// Create a new <Compile Include="..." /> node
  /// </summary>
  /// <param name="doc">XmlDocument instance to for which we want to create the node</param>
  /// <param name="includePath">Name of the path to include</param>
  /// <returns>
  /// Returns a new <Compile Include="..." /> node
  /// </returns>
  let private createNewCompileNode (doc: System.Xml.XmlDocument) (includePath: string) =
    let node = doc.CreateElement("Compile")
    node.SetAttribute("Include", includePath)
    node

  let moveFileUp (fsprojPath: string) (file: string) =
    let xDoc = System.Xml.XmlDocument()
    xDoc.PreserveWhitespace <- true // to keep custom formatting if present
    xDoc.Load fsprojPath
    let xpath = sprintf "//Compile[@Include='%s']/.." file
    let itemGroup = xDoc.SelectSingleNode(xpath)
    let childXPath = sprintf "//Compile[@Include='%s']" file
    let node = itemGroup.SelectSingleNode(childXPath)
    let upNode = node.PreviousSibling

    if isNull upNode then
      ()
    else
      itemGroup.RemoveChild node |> ignore
      itemGroup.InsertBefore(node, upNode) |> ignore
      xDoc.Save fsprojPath

  let moveFileDown (fsprojPath: string) (file: string) =
    let xDoc = System.Xml.XmlDocument()
    xDoc.PreserveWhitespace <- true // to keep custom formatting if present
    xDoc.Load fsprojPath
    let xpath = sprintf "//Compile[@Include='%s']/.." file
    let itemGroup = xDoc.SelectSingleNode(xpath)
    let childXPath = sprintf "//Compile[@Include='%s']" file
    let node = itemGroup.SelectSingleNode(childXPath)
    let downNode = node.NextSibling

    if isNull downNode then
      ()
    else
      itemGroup.RemoveChild node |> ignore
      itemGroup.InsertAfter(node, downNode) |> ignore
      xDoc.Save fsprojPath

  let addFileAbove (fsprojPath: string) (aboveFile: string) (newFileName: string) =
    let xDoc = System.Xml.XmlDocument()
    xDoc.Load fsprojPath

    if fileAlreadyIncludedViaCompileTag xDoc newFileName then
      Error "File already included in the project"
    else
      let xpath = sprintf "//Compile[@Include='%s']/.." aboveFile
      let itemGroup = xDoc.SelectSingleNode(xpath)
      let childXPath = sprintf "//Compile[@Include='%s']" aboveFile
      let aboveNode = itemGroup.SelectSingleNode(childXPath)
      let node = createNewCompileNode xDoc newFileName
      itemGroup.InsertBefore(node, aboveNode) |> ignore
      xDoc.Save fsprojPath
      Ok()

  let addFileBelow (fsprojPath: string) (belowFile: string) (newFileName: string) =
    let xDoc = System.Xml.XmlDocument()
    xDoc.PreserveWhitespace <- true // to keep custom formatting if present
    xDoc.Load fsprojPath

    if fileAlreadyIncludedViaCompileTag xDoc newFileName then
      Error "File already included in the project"
    else
      let xpath = sprintf "//Compile[@Include='%s']/.." belowFile
      let itemGroup = xDoc.SelectSingleNode(xpath)
      let childXPath = sprintf "//Compile[@Include='%s']" belowFile
      let aboveNode = itemGroup.SelectSingleNode(childXPath)
      let node = createNewCompileNode xDoc newFileName
      itemGroup.InsertAfter(node, aboveNode) |> ignore
      xDoc.Save fsprojPath
      Ok()

  let renameFile (fsprojPath: string) (oldFileName: string) (newFileName: string) =
    let xDoc = System.Xml.XmlDocument()
    xDoc.PreserveWhitespace <- true // to keep custom formatting if present
    xDoc.Load fsprojPath
    let xpath = sprintf "//Compile[@Include='%s']" oldFileName
    let node = xDoc.SelectSingleNode(xpath)
    node.Attributes["Include"].InnerText <- newFileName
    xDoc.Save fsprojPath

  let addFile (fsprojPath: string) (newFileName: string) =
    let xDoc = System.Xml.XmlDocument()
    xDoc.PreserveWhitespace <- true // to keep custom formatting if present
    xDoc.Load fsprojPath

    if fileAlreadyIncludedViaCompileTag xDoc newFileName then
      Error "File already included in the project"
    else
      let newNode = createNewCompileNode xDoc newFileName

      let compileItemGroups =
        xDoc.SelectNodes("//Compile/.. | //None/.. | //EmbeddedResource/.. | //Content/..")

      let hasExistingCompileElement = compileItemGroups.Count > 0

      if hasExistingCompileElement then
        let firstCompileItemGroup =
          compileItemGroups |> Seq.cast<System.Xml.XmlNode> |> Seq.head

        let x = firstCompileItemGroup.FirstChild

        firstCompileItemGroup.InsertBefore(newNode, x) |> ignore
      else
        let itemGroup = xDoc.CreateElement("ItemGroup")
        itemGroup.AppendChild(newNode) |> ignore
        let projectNode = xDoc.SelectSingleNode("//Project")
        projectNode.AppendChild(itemGroup) |> ignore

      xDoc.Save fsprojPath
      Ok()

  let addExistingFile (fsprojPath: string) (existingFile: string) =
    let relativePath =
      Path.GetRelativePath(Path.GetDirectoryName fsprojPath, existingFile)

    addFile fsprojPath relativePath

  let removeFile (fsprojPath: string) (fileToRemove: string) =
    let xDoc = System.Xml.XmlDocument()
    xDoc.PreserveWhitespace <- true // to keep custom formatting if present
    xDoc.Load fsprojPath
    let sanitizedFileToRemove = fileToRemove.Replace("\\", "/")

    let nodeToRemoveOpt =
      // Take all the <Compile Include="..." /> nodes
      xDoc.SelectNodes("//Compile[@Include]")
      |> Seq.cast<System.Xml.XmlNode>
      // Find the node that match the file we want to remove
      // Note: We sanitize the file name path because it can changes depending on the OS
      // and also on if the user used / or \ as the separator
      |> Seq.tryFind (fun node ->
        let sanitizedInclude = node.Attributes.["Include"].InnerText.Replace("\\", "/")
        sanitizedInclude = sanitizedFileToRemove)

    match nodeToRemoveOpt with
    | Some nodeToRemove ->
      nodeToRemove.ParentNode.RemoveChild(nodeToRemove) |> ignore
      xDoc.Save fsprojPath

    | None ->
      // Node not found, do nothing
      ()
