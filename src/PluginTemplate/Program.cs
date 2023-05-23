using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace CustomChatChannels
{
    [ApiVersion(2, 1)]
    public class CustomChatChannelsPlugin : TerrariaPlugin
    {
        private Dictionary<string, string> playerChannels;
        private Dictionary<string, Color> channelColors;
        private string configFile = Path.Combine(TShock.SavePath, "CustomChatChannelsConfig.json");

        public override string Name => "CustomChatChannels";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "YourName";
        public override string Description => "Allows players to create and manage custom chat channels.";

        public CustomChatChannelsPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            playerChannels = new Dictionary<string, string>();
            channelColors = new Dictionary<string, Color>();

            TShockAPI.Commands.ChatCommands.Add(new Command("customchat.channel.create", CreateChannel, "createchannel"));
            TShockAPI.Commands.ChatCommands.Add(new Command("customchat.channel.join", JoinChannel, "joinchannel"));
            TShockAPI.Commands.ChatCommands.Add(new Command("customchat.channel.leave", LeaveChannel, "leavechannel"));
            TShockAPI.Commands.ChatCommands.Add(new Command("customchat.channel.chat", ChatChannel, "chat", "c"));
            TShockAPI.Commands.ChatCommands.Add(new Command("customchat.channel.list", ListChannels, "listchannels"));
            TShockAPI.Commands.ChatCommands.Add(new Command("customchat.channel.color", SetChannelColor, "setchannelcolor"));
            ServerApi.Hooks.ServerChat.Register(this, OnChat);

            LoadConfig();
        }

        private void OnChat(ServerChatEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player == null || !playerChannels.ContainsKey(player.Name))
                return;

            var channel = playerChannels[player.Name];
            if (channel == null)
                return;

            var channelColor = channelColors.ContainsKey(channel) ? channelColors[channel] : Color.White;
            var message = $"[{channel}] {player.Name}: {args.Text}";

            TSPlayer.All.SendMessage(message, channelColor);

            args.Handled = true;
        }

        private void CreateChannel(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /createchannel <channelname>");
                return;
            }

            var channelName = args.Parameters[0];

            if (playerChannels.ContainsKey(channelName))
            {
                args.Player.SendErrorMessage($"Channel '{channelName}' already exists.");
                return;
            }

            playerChannels[args.Player.Name] = channelName;
            args.Player.SendSuccessMessage($"Created and joined channel '{channelName}'.");
        }

        private void JoinChannel(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /joinchannel <channelname>");
                return;
            }

            var channelName = args.Parameters[0];

            if (!playerChannels.ContainsKey(channelName))
            {
                args.Player.SendErrorMessage($"Channel '{channelName}' does not exist.");
                return;
            }

            playerChannels[args.Player.Name] = channelName;
            args.Player.SendSuccessMessage($"Joined channel '{channelName}'.");
        }

        private void LeaveChannel(CommandArgs args)
        {
            if (!playerChannels.ContainsKey(args.Player.Name))
            {
                args.Player.SendErrorMessage("You are not in a channel.");
                return;
            }

            var channel = playerChannels[args.Player.Name];
            playerChannels.Remove(args.Player.Name);
            args.Player.SendSuccessMessage($"Left channel '{channel}'.");
        }

        private void ChatChannel(CommandArgs args)
        {
            if (!playerChannels.ContainsKey(args.Player.Name))
            {
                args.Player.SendErrorMessage("You are not in a channel.");
                return;
            }

            var channel = playerChannels[args.Player.Name];
            var channelColor = channelColors.ContainsKey(channel) ? channelColors[channel] : Color.White;
            var message = string.Join(" ", args.Parameters);

            TSPlayer.All.SendMessage($"[{channel}] {args.Player.Name}: {message}", channelColor);
        }

        private void ListChannels(CommandArgs args)
        {
            var channelList = string.Join(", ", playerChannels.Values);
            args.Player.SendInfoMessage($"Channels: {channelList}");
        }

        private void SetChannelColor(CommandArgs args)
        {
            if (args.Parameters.Count < 4)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /setchannelcolor <channelname> <R> <G> <B>");
                return;
            }

            var channelName = args.Parameters[0];
            var rStr = args.Parameters[1];
            var gStr = args.Parameters[2];
            var bStr = args.Parameters[3];

            if (!playerChannels.ContainsKey(channelName))
            {
                args.Player.SendErrorMessage($"Channel '{channelName}' does not exist.");
                return;
            }

            if (!int.TryParse(rStr, out var r) || !int.TryParse(gStr, out var g) || !int.TryParse(bStr, out var b))
            {
                args.Player.SendErrorMessage("Invalid color values specified.");
                return;
            }

            var color = new Color(r, g, b);
            channelColors[channelName] = color;

            args.Player.SendSuccessMessage($"Set color for channel '{channelName}' to ({r}, {g}, {b}).");
        }

        private void LoadConfig()
        {
            if (File.Exists(configFile))
            {
                try
                {
                    var json = File.ReadAllText(configFile);
                    channelColors = JsonConvert.DeserializeObject<Dictionary<string, Color>>(json);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error loading configuration:");
                    Console.WriteLine(ex);
                    Console.ResetColor();
                }
            }

            if (channelColors == null)
                channelColors = new Dictionary<string, Color>();
        }

        private void SaveConfig()
        {
            try
            {
                var json = JsonConvert.SerializeObject(channelColors, Formatting.Indented);
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error saving configuration:");
                Console.WriteLine(ex);
                Console.ResetColor();
            }
        }

        public void DeInitialize()
        {
            playerChannels.Clear();
            channelColors.Clear();
            SaveConfig();
        }

    }
}
