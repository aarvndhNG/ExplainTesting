namespace CustomChatPlugin

open System
open System.IO
open System.Text.RegularExpressions
open Terraria
open TerrariaApi.Server
open TShockAPI

[<AutoLoad>]
type Plugin() =

    let mutable _config : Config = Config()
    let mutable _prefix : string = ""

    let sendMessageCommand =
        { name = "send"
          permissions = [|"customchat.send"|]
          callback = fun args ->
              match args.Parameters with
              | [|message|] ->
                  let formattedMessage = _config.chatFormat.Replace("{player}", args.Player.Name).Replace("{message}", message)
                  TShock.Utils.Broadcast(formattedMessage, Color.White)
              | _ -> TShock.Utils.SendErrorMessage(args.Player, "Invalid syntax! Proper syntax: /send [message]")
          helpText = "Usage: /send [message]. Sends a custom chat message." 
          subCommands = [] }

    let onInitialize(_eventData : InitializationEventArgs) =
        // Load configuration
        _config <- Config.load()

        // Register plugin commands
        Commands.ChatCommands.Add(sendMessageCommand.name, sendMessageCommand.callback, sendMessageCommand.permissions)

        // Set prefix from configuration
        _prefix <- _config.chatPrefix

    let onChat(_eventData : TerrariaApi.Server.ServerChatEventArgs) =
        // Check if the message starts with the configured prefix
        let message = _eventData.Text.Trim()
        if message.StartsWith(_prefix) then
            // Cancel the message event to prevent it from being sent to other players
            _eventData.Handled <- true

            // Parse the message and send the custom chat message
            let customMessage = message.Substring(_prefix.Length)
            let formattedMessage = _config.chatFormat.Replace("{player}", _eventData.Player.Name).Replace("{message}", customMessage)
            TShock.Utils.Broadcast(formattedMessage, Color.White)

    // Plugin methods
    member __.Initialize() =
        // Register plugin hooks
        ServerApi.Hooks.GameInitialize.Register(this, onInitialize)
        ServerApi.Hooks.ServerChat.Register(this, onChat)

    member __.Dispose() =
        // Unregister plugin hooks
        ServerApi.Hooks.GameInitialize.Deregister(this, onInitialize)
        ServerApi.Hooks.ServerChat.Deregister(this, onChat)

// Configuration file
type Config() =
    let mutable _chatPrefix : string = "!"
    let mutable _chatFormat : string = "[{player}]: {message}"

    member this.chatPrefix
        with get() = _chatPrefix
        and set(value) = _chatPrefix <- value

    member this.chatFormat
        with get() = _chatFormat
        and set(value) = _chatFormat <- value

    static member default() = Config()

    static member load() =
        let configFile = Path.Combine(TShock.SavePath, "CustomChatPlugin.json")
        if not (File.Exists(configFile)) then
            let config = Config.default()
            File.WriteAllText(configFile, JsonConvert.SerializeObject(config, Formatting.Indented))
            config
        else
            JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile)) |> Option.defaultValue(Config.default())
