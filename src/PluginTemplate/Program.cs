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
        public override string Name => "PlayerListPlugin";
        public override string Author => "YourName";
        public override string Description => "A plugin to list players who joined the server.";
        public override Version Version => new Version(1, 0, 0);

        public PlayerListPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("playerlist.list", ListPlayers, "list"));
            TShockAPI.Commands.ChatCommands.Add(new Command("playerlist.all", ListAllPlayers, "listall"));
        }

        private void ListPlayers(CommandArgs args)
        {
            TSPlayer.All.SendInfoMessage("Players who joined previously:");

            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.IsLoggedIn)
                {
                    TSPlayer.All.SendInfoMessage(player.Name);
                }
            }
        }

        private void ListAllPlayers(CommandArgs args)
        {
            TSPlayer.All.SendInfoMessage("All registered players on the server:");

            foreach (TShockUser user in TShock.Users.GetAllUsers())
            {
                TSPlayer.All.SendInfoMessage(user.Name);
            }
        }
    }
}
