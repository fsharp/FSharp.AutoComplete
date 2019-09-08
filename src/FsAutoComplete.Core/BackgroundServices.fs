module BackgroundServices

open FsAutoComplete
open LanguageServerProtocol
open System.IO
open FSharp.Compiler.SourceCodeServices

type Msg = {Value: string}

type UpdateFileParms = {
    File: string
    Content: string
    Version: int
}

type ProjectParms = {
    Options: FSharpProjectOptions
    File: string
}

type FileParms = {
    File: string
}

let p =
    let t = typeof<State>
    Path.GetDirectoryName t.Assembly.Location

let pid =
    System.Diagnostics.Process.GetCurrentProcess().Id.ToString()

type MessageType =
    | Diagnostics of Types.PublishDiagnosticsParams

let messageRecived = Event<MessageType>()

let client =

    let notificationsHandler =
        Map.empty
        |> Map.add "background/notify" (Client.notificationHandling (fun (msg: Msg) -> async {
            Debug.print "[BACKGROUND SERVICE] Msg: %s" msg.Value
            return None
        } ))
        |> Map.add "background/diagnostics" (Client.notificationHandling (fun (msg: Types.PublishDiagnosticsParams) -> async {
            messageRecived.Trigger (Diagnostics msg)
            return None
        } ))

    #if DOTNET_SPAWN
    Client.Client("dotnet", Path.Combine(p, "fsautocomplete.backgroundservices.dll") + " " + pid, notificationsHandler)
    #else
    if Utils.runningOnMono then
        Client.Client("mono", Path.Combine(p, "fsautocomplete.backgroundservices.exe")+ " " + pid, notificationsHandler)
    else
        Client.Client(Path.Combine(p, "fsautocomplete.backgroundservices.exe"), pid, notificationsHandler)
    #endif

let start () =
    client.Start ()

let updateFile(file, content, version) =
    let msg = {File = file; Content = content; Version = version}
    client.SendRequest "background/update" msg

let updateProject(file, opts) =
    let msg = {File = file; Options = opts}
    client.SendRequest "background/project" msg

let saveFile(file) =
    let msg = {File = file}
    client.SendRequest "background/save" msg