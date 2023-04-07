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
        private string _configPath = Path.Combine(TShock.SavePath, "CustomMessagesPlugin.json");

        public override string Name => "CustomMessagesPlugin";
        public override string Author => "YourName";
        public override string Description => "Allows custom chat and join/leave messages.";
        public override Version Version => new Version(1, 0, 0);

        public CustomMessagesPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);

            LoadConfig();
        }

        public override void DeInitialize()
        {
            ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
            ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    _config = new Config();
                    SaveConfig();
                }
                else
                {
                    using (var reader = new StreamReader(_configPath))
                    {
                        var json = reader.ReadToEnd();
                        _config = JsonConvert.DeserializeObject<Config>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
        }

        private void SaveConfig()
        {
            try
            {
                using (var writer = new StreamWriter(_configPath))
                {
                    var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                    writer.Write(json);
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
        }

        private void OnChat(ServerChatEventArgs args)
        {
            if (args.Handled)
                return;

            var message = args.Text;

            if (_config.ChatMessages.TryGetValue(message, out string response))
            {
                TShock.Utils.Broadcast(response, _config.ChatMessageColor);
                args.Handled = true;
            }
        }

        private void OnJoin(JoinEventArgs args)
        {
            var message = _config.JoinMessage.Replace("{player}", args.Who.ToString());
            TShock.Utils.Broadcast(message, _config.JoinLeaveMessageColor);
        }

        private void OnLeave(LeaveEventArgs args)
        {
            var message = _config.LeaveMessage.Replace("{player}", args.Who.ToString());
            TShock.Utils.Broadcast(message, _config.JoinLeaveMessageColor);
        }

        private class Config
        {
            public Dictionary<string, string> ChatMessages { get; set; } = new Dictionary<string, string>();
            public string ChatMessageColor { get; set; } = "yellow";
            public string JoinMessage { get; set; } = "{player} joined the server.";
            public string LeaveMessage { get; set; } = "{player} left the server.";
            public string JoinLeaveMessageColor { get; set; } = "white";
        }
    }
}
