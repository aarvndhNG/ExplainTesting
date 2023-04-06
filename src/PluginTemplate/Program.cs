using System;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace BotPlugin
{
    [ApiVersion(2, 1)]
    public class BotPlugin : TerrariaPlugin
    {
        public override string Name => "BotPlugin";
        public override string Author => "Your Name";
        public override string Description => "A bot helper for players.";
        public override Version Version => new Version(1, 0, 0);

        public BotPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            TShockAPI.Commands.ChatCommands.Add(new Command("bot.help", BotHelp, "tshelp"));
        }

        private void OnJoin(JoinEventArgs e)
        {
            TShock.Utils.Broadcast($"{_config.WelcomeMessage} {e.Who}!");
        }


        private void BotHelp(CommandArgs args)
        {
            // List of available bot commands
            var message = "Available bot commands: !tptime, !tpme, !tpall, !healme, !healall";
            args.Player.SendMessage(message, Color.Yellow);
        }

        private void TeleportPlayer(CommandArgs args, int targetIndex)
        {
            var target = TShock.Players[targetIndex];
            if (target == null || !target.Active)
            {
                args.Player.SendErrorMessage("Invalid player.");
                return;
            }

            var player = args.Player;
            var x = target.TileX * 16 + 8;
            var y = target.TileY * 16;
            player.Teleport(x, y);
            player.SendSuccessMessage($"Teleported to {target.Name}.");
        }
        
        private static BotConfig _config;

    public override void Initialize()
    {
        ReadConfig();
        // ...
    }

    private void ReadConfig()
    {
        var configFile = Path.Combine(TShock.SavePath, "bot_config.json");
        if (!File.Exists(configFile))
        {
            _config = new BotConfig();
            using (var writer = new StreamWriter(configFile))
            {
                writer.Write(JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
        }
        else
        {
            using (var reader = new StreamReader(configFile))
            {
                _config = JsonConvert.DeserializeObject<BotConfig>(reader.ReadToEnd());
        
        }

        private void TeleportAll(CommandArgs args)
        {
            var player = args.Player;
            foreach (var tsPlayer in TShock.Players)
            {
                if (tsPlayer != null && tsPlayer.Active && tsPlayer != player)
                {
                    TeleportPlayer(args, tsPlayer.Index);
                }
            }
            player.SendSuccessMessage("Teleported all players.");
        }

        private void HealPlayer(CommandArgs args, int targetIndex)
        {
            var target = TShock.Players[targetIndex];
            if (target == null || !target.Active)
            {
                args.Player.SendErrorMessage("Invalid player.");
                return;
            }

            target.Heal();
            target.SendSuccessMessage($"You were healed by {args.Player.Name}.");
            args.Player.SendSuccessMessage($"Healed {target.Name}.");
        }

        private void HealAll(CommandArgs args)
        {
            var player = args.Player;
            foreach (var tsPlayer in TShock.Players)
            {
                if (tsPlayer != null && tsPlayer.Active && tsPlayer != player)
                {
                    HealPlayer(args, tsPlayer.Index);
                }
            }
            player.SendSuccessMessage("Healed all players.");
        }
    }
}

