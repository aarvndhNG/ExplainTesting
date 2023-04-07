using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace CustomMessagesPlugin
{
    [ApiVersion(2, 1)]
    public class CustomMessagesPlugin : TerrariaPlugin
    {
        private Config _config;

        public override string Name => "CustomMessagesPlugin";
        public override string Author => "YourName";
        public override string Description => "Custom join, leave, and chat messages.";
        public override Version Version => new Version(1, 0, 0);

        public CustomMessagesPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            TShockAPI.Hooks.GameHooks.PostGetData.Register(this, OnPostGetData);
            LoadConfig();
        }

        public override void DeInitialize()
        {
            ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
            ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            TShockAPI.Hooks.GameHooks.PostGetData.Deregister(this, OnPostGetData);
            SaveConfig();
        }

        private void LoadConfig()
        {
            _config = Config.Read();
        }

        private void SaveConfig()
        {
            _config.Write();
        }

        private void OnServerJoin(JoinEventArgs args)
        {
            TShock.Utils.Broadcast(string.Format(_config.JoinMessage, args.Who, TShock.Players[args.Who].Name), _config.JoinColor);
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
            TShock.Utils.Broadcast(string.Format(_config.LeaveMessage, args.Who, TShock.Players[args.Who].Name), _config.LeaveColor);
        }

        private void OnPostGetData(GetDataEventArgs args)
        {
            if (args.Handled)
                return;

            if (args.MsgID == PacketTypes.ChatText && args.number1 >= 0 && args.number1 < 255)
            {
                var player = TShock.Players[args.number1];
                if (player == null)
                    return;

                var message = args.readBuffer.ReadString().Trim();
                if (string.IsNullOrWhiteSpace(message))
                    return;

                TShock.Utils.Broadcast(string.Format(_config.ChatMessage, player.Name, message), _config.ChatColor);
            }
        }
    }

    internal class Config
    {
        public string JoinMessage { get; set; } = "{1} ({0}) has joined the server.";
        public string JoinColor { get; set; } = "yellow";

        public string LeaveMessage { get; set; } = "{1} ({0}) has left the server.";
        public string LeaveColor { get; set; } = "yellow";

        public string ChatMessage { get; set; } = "{0}: {1}";
        public string ChatColor { get; set; } = "white";

        public static Config Read()
        {
            return new Config();
        }

        public void Write()
        {
        }
    }
}
