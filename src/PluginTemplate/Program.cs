﻿using System;
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
        
        public class BotConfig
        {
            public string BotName { get; set; } = "Bot";
            public string BotPrefix { get; set; } = "!";
            public string BotCommandChar { get; set; } = "/";
            public string BotCommandCharAlias { get; set; } = "!";
            public string BotCommandCharAlias2 { get; set; } = ".";
            public bool BotAnnounceConnections { get; set; } = true;
            public string BotAnnouncementFormat { get; set; } = "[{0}] {1} has joined the server.";
            public bool BotAnnounceDisconnections { get; set; } = true;
            public string BotDisconnectionFormat { get; set; } = "[{0}] {1} has left the server.";
            public bool BotLogChat { get; set; } = true;
            public string BotLogChatFormat { get; set; } = "[{0}] [{1}] {2}: {3}";
            public bool BotLogCommands { get; set; } = true;
            public string BotLogCommandsFormat { get; set; } = "[{0}] {1}: {2}";
            public bool BotLogWarnings { get; set; } = true;
            public string BotLogWarningsFormat { get; set; } = "[{0}] WARNING: {1}";
            public string BotCommandPrefix { get; set; } = "!";
            public string BotCommandSuffix { get; set; } = "";
            public bool BotAutoSave { get; set; } = true;
            public int BotAutoSaveInterval { get; set; } = 600;
            public bool BotAutoRestart { get; set; } = false;
            public int BotAutoRestartInterval { get; set; } = 3600;
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

