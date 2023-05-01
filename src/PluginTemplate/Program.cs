using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;
using Terraria;

namespace AutoBroadcastPlugin
{
    [ApiVersion(2, 1)]
    public class AutoBroadcastPlugin : TerrariaPlugin
    {
        private DateTime lastBroadcastTime;
        private int broadcastInterval;
        private List<string> broadcastMessages;
        private Color broadcastColor;
        private Color broadcastPrefixColor;

        public override string Name => "AutoBroadcastPlugin";
        public override string Author => "Your Name";
        public override string Description => "Automatically broadcasts a message at a specified interval.";
        public override Version Version => new Version(1, 0, 0);

        public AutoBroadcastPlugin(Main game) : base(game)
        {
            lastBroadcastTime = DateTime.Now;
            broadcastInterval = 60; // 1 minute
            broadcastMessages = new List<string> { "Welcome to our server!", "Thanks for playing!", "Don't forget to vote!", "Join our Discord server!" };
            broadcastColor = Color.Yellow;
            broadcastPrefixColor = Color.White;
        }

        public override void Initialize()
        {
            TShockAPI.Hooks.GeneralHooks.Update += OnUpdate;

            string configPath = Path.Combine(TShock.SavePath, "AutoBroadcastPlugin.json");
            if (File.Exists(configPath))
            {
                string configJson = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<Config>(configJson);
                broadcastInterval = config.BroadcastInterval;
                broadcastMessages = config.BroadcastMessages;
                broadcastColor = new Color(config.BroadcastColorR, config.BroadcastColorG, config.BroadcastColorB);
                broadcastPrefixColor = new Color(config.BroadcastPrefixColorR, config.BroadcastPrefixColorG, config.BroadcastPrefixColorB);
            }
            else
            {
                var config = new Config();
                string configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, configJson);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TShockAPI.Hooks.GeneralHooks.Update -= OnUpdate;
            }
            base.Dispose(disposing);
        }

        private void OnUpdate(EventArgs args)
        {
            if ((DateTime.Now - lastBroadcastTime).TotalSeconds >= broadcastInterval)
            {
                string message = broadcastMessages[new Random().Next(broadcastMessages.Count)];
                TShock.Utils.Broadcast(message, broadcastColor, broadcastPrefixColor);
                lastBroadcastTime = DateTime.Now;
            }
        }

        private class Config
        {
            public int BroadcastInterval { get; set; } = 60;
            public List<string> BroadcastMessages { get; set; } = new List<string> { "Welcome to our server!", "Thanks for playing!", "Don't forget to vote!", "Join our Discord server!" };
            public byte BroadcastColorR { get; set; } = 255;
            public byte BroadcastColorG { get; set; } = 255;
            public byte BroadcastColorB { get; set; } = 0;
            public byte BroadcastPrefixColorR { get; set; } = 255;
            public byte BroadcastPrefixColorG { get; set; } = 255;
            public byte BroadcastPrefixColorB { get; set; } = 255;
        }
    }
}
