using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerListPlugin
{
    [ApiVersion(2, 1)]
    public class PlayerListPlugin : TerrariaPlugin
    {
        private List<string> joinedPlayers = new List<string>();

        public override string Name => "PlayerListPlugin";
        public override Version Version => new Version(1, 1, 0);
        public override string Author => "YourName";
        public override string Description => "Lists players who have joined the server.";

        public PlayerListPlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            Commands.ChatCommands.Add(new Command("playerlist.list", ListPlayers, "list"));
        }

        private void OnJoin(JoinEventArgs e)
        {
            if (TShock.Players[e.Who] != null && TShock.Players[e.Who].Active)
            {
                joinedPlayers.Add(TShock.Players[e.Who].Name);
            }
        }

        private void OnLeave(LeaveEventArgs e)
        {
            if (TShock.Players[e.Who] != null && TShock.Players[e.Who].Active)
            {
                joinedPlayers.Remove(TShock.Players[e.Who].Name);
            }
        }

        private void ListPlayers(CommandArgs args)
        {
            int playerCount = TShock.Players.Where(p => p != null && p.Active).Count();
            string playerNames = string.Join(", ", joinedPlayers);
            args.Player.SendInfoMessage($"Players ({playerCount}): {playerNames}");
        }
    }
}
