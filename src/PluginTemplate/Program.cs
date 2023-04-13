using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace MyPlugin
{
    [ApiVersion(2, 1)]
    public class MyPlugin : TerrariaPlugin
    {
        private Dictionary<string, string> Responses = new Dictionary<string, string>()
        {
            { "hello", "Hello there!" },
            { "how are you", "I'm doing great, thanks for asking." },
            { "help", "How can I help you?" }
        };

        public override string Author => "Your name";
        public override string Name => "MyPlugin";
        public override string Description => "My awesome TShock plugin.";
        public override Version Version => new Version(1, 0, 0);

        public MyPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
        }

        private void OnChat(ServerChatEventArgs args)
        {
            string message = args.Text;
            string response = GetResponse(message.ToLower());
            if (!string.IsNullOrEmpty(response))
            {
                TShock.Utils.Broadcast($"[Chat Assistant] {response}", Color.Orange);
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
