using System;
using System.Collections.Generic;
using static System.Drawing.Color;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Microsoft.Xna.Framework.Color;
using Newtonsoft.Json;

namespace MyPlugin
{
    [ApiVersion(2, 1)]
    public class MyPlugin : TerrariaPlugin
    {
        private Dictionary<string, string> Responses;

        public override string Author => "Your name";
        public override string Name => "MyPlugin";
        public override string Description => "My awesome TShock plugin.";
        public override Version Version => new Version(1, 0, 0);

        public MyPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            LoadConfig();
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(TShock.SavePath, "MyPluginConfig.json");
            if (File.Exists(configPath))
            {
                string configText = File.ReadAllText(configPath);
                Responses = JsonConvert.DeserializeObject<Dictionary<string, string>>(configText);
            }
            else
            {
                Responses = new Dictionary<string, string>();
            }
        }

        private void OnChat(ServerChatEventArgs args)
        {
            string message = args.Text;
            string response = GetResponse(message.ToLower());
            if (!string.IsNullOrEmpty(response))
            {
                TShock.Utils.Broadcast($"[Chat Assistant] {response}", Microsoft.Xna.Framework.Color.Orange);

            }
        }

        private string GetResponse(string message)
        {
            foreach (KeyValuePair<string, string> pair in Responses)
            {
                if (message.Contains(pair.Key))
                {
                    return pair.Value;
                }
            }
            return null;
        }
    }
}
