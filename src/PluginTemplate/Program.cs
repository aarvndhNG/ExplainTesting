using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace AIBot
{
    [ApiVersion(2, 1)]
    public class AIBotPlugin : TerrariaPlugin
    {
        private static AIChatBot _bot;

        public override string Name => "AIBot Plugin";
        public override string Author => "Your Name";
        public override string Description => "An AI functional bot for your TShock server";
        public override Version Version => new Version(1, 0, 0);

        public AIBotPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            _bot = new AIChatBot();
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
        }

        private void OnServerChat(ServerChatEventArgs args)
        {
            if (!args.Handled && args.Text.StartsWith(TShockAPI.TShock.Config.CommandSpecifier))
            {
                var text = args.Text.Substring(1);
                var parts = text.Split(' ');

                if (parts.Length == 1 && parts[0] == "hello")
                {
                    TShock.Utils.Broadcast($"{args.Who} says hello to {_bot.Name}");
                    args.Handled = true;
                }
                else
                {
                    var reply = _bot.GenerateReply(text, args.Who);
                    if (!string.IsNullOrEmpty(reply))
                    {
                        TShock.Utils.Broadcast($"{_bot.Name}: {reply}");
                        args.Handled = true;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this, OnServerChat);
            }

            base.Dispose(disposing);
        }
    }

    public class AIChatBot
    {
        private readonly string[] _greetings =
        {
            "Hi there!",
            "Hello!",
            "Greetings!",
            "Howdy!",
            "Hey!"
        };

        private readonly string[] _goodbyes =
        {
            "Goodbye!",
            "See you later!",
            "Farewell!",
            "Take care!",
            "Until next time!"
        };

        private readonly string[] _thanks =
        {
            "You're welcome!",
            "No problem!",
            "Happy to help!",
            "Anytime!",
            "Glad to be of service!"
        };

        private readonly string[] _confused =
        {
            "I'm sorry, I don't understand.",
            "Could you please rephrase that?",
            "I'm not sure what you mean.",
            "Can you please clarify?",
            "I don't know what to say to that."
        };

        private readonly Random _random = new Random();
        public string Name { get; set; }

        public AIChatBot()
        {
            Name = "AIBot";
        }

        public string GenerateReply(string message, int playerId)
        {
            if (message.EndsWith("?"))
            {
                return $"{_random.Choose(_confused)}";
            }
            else if (message.Contains("hello"))
            {
                return $"{_random.Choose(_greetings)}";
            }
            else if (message.Contains("bye"))
            {
                return $"{_random.Choose(_goodbyes)}";
            }
            else if (message.Contains("thank"))
            {
                return $"{_random.Choose(_thanks)}";
            }
            else
            {
                return string.Empty;
            }
        }
    }

 public static class Extensions
{
    public static T Choose<T>(this Random random, IEnumerable<T> source)
    {
        var count = source.Count();
        var index = random.Next(count);
        return source.ElementAt(index);
    }
}

    
