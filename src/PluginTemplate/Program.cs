using System;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace PlayerListPlugin
{
    [ApiVersion(2, 1)]
    public class PlayerListPlugin : TerrariaPlugin
    {
        public override string Name => "PlayerListPlugin";
        public override string Author => "Blackwolf";
        public override string Description => "A plugin to list registered players on the server.";
        public override Version Version => new Version(1, 0, 0);

        public PlayerListPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            }
            base.Dispose(disposing);
        }

        private void OnInitialize(EventArgs args)
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("playerlist.listall", ListAllPlayers, "listall"));
        }

        private void ListAllPlayers(CommandArgs args)
        {
            if (args.Player == null)
            {
                return;
            }

            TSPlayer.All.SendInfoMessage("All registered players on the server:");

            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.IsLoggedIn)
                {
                    TSPlayer.All.SendInfoMessage(player.Name);
                }
            }
        }
    }
}
