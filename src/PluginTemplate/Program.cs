using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using Discord;
using Discord.WebSocket;

namespace ChatBotPlugin
{
    [ApiVersion(2, 1)]
    public class ChatBotPlugin : TerrariaPlugin
    {
        private DiscordSocketClient _client;
        private string _token = "YOUR_DISCORD_BOT_TOKEN_HERE";

        public override string Name => "Chat Bot Plugin";
        public override string Author => "Your Name";
        public override string Description => "A plugin that adds a chat bot to the server.";
        public override Version Version => new Version(1, 0, 0);

        public ChatBotPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);

            _client = new DiscordSocketClient();
            _client.MessageReceived += OnDiscordMessageReceived;
            _client.LoginAsync(TokenType.Bot, _token).Wait();
            _client.StartAsync().Wait();
        }

        private void OnServerChat(ServerChatEventArgs args)
        {
            if (args.Text.StartsWith("!help"))
            {
                _client.GetGuild(1234567890).GetTextChannel(1234567890).SendMessageAsync("Welcome to the server! Here are some helpful commands:");
                _client.GetGuild(1234567890).GetTextChannel(1234567890).SendMessageAsync("!rules - Displays the server rules.");
                _client.GetGuild(1234567890).GetTextChannel(1234567890).SendMessageAsync("!players - Displays the current players on the server.");
                _client.GetGuild(1234567890).GetTextChannel(1234567890).SendMessageAsync("!warp - Displays the available warp points.");
            }
        }

        private async Task OnDiscordMessageReceived(SocketMessage message)
        {
            if (message.Content.StartsWith("!players"))
            {
                string playerList = "";
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player != null && player.Active)
                    {
                        playerList += $"{player.Name}, ";
                    }
                }
                await message.Channel.SendMessageAsync($"Current players: {playerList}");
            }
        }
    }
}
