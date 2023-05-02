using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.Collections.Generic;

namespace RandomTeleportPlugin
{
    [ApiVersion(2, 1)]
    public class RandomTeleportPlugin : TerrariaPlugin
    {
        public override string Name => "RandomTeleportPlugin";
        public override string Author => "Your Name Here";
        public override string Description => "Teleports players to a random location on the map";
        public override Version Version => new Version(1, 0, 0);

        public RandomTeleportPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("rtp.teleport", RandomTeleport, "rtp"));
        }

        private void RandomTeleport(CommandArgs args)
        {
            TSPlayer player = args.Player;
            if (player == null || !player.Active)
            {
                return;
            }

            int x = Main.rand.Next(0, Main.maxTilesX);
            int y = Main.rand.Next(0, Main.maxTilesY);
            int spawnTileX = (int)WorldGen.spawnTileX;
            int spawnTileY = (int)WorldGen.spawnTileY;
            int maxTries = 100;

            while (!CanTeleport(x, y) && maxTries > 0)
            {
                x = Main.rand.Next(0, Main.maxTilesX);
                y = Main.rand.Next(0, Main.maxTilesY);
                maxTries--;
            }

            if (maxTries == 0)
            {
                player.SendErrorMessage("Failed to find a safe location to teleport");
                return;
            }

            player.Teleport(x * 16, y * 16);
            player.SendSuccessMessage("Teleported to a random location");
        }

        private bool CanTeleport(int x, int y)
        {
            List<Point16> points = new List<Point16>();

            // Check if the area around the teleport point is safe
            for (int i = x - 10; i < x + 10; i++)
            {
                for (int j = y - 10; j < y + 10; j++)
                {
                    if (!WorldGen.SolidTile(i, j) && !Main.tile[i, j].lava() && !Main.tile[i, j].liquid)
                    {
                        points.Add(new Point16((short)i, (short)j));
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // Check if there are any players or NPCs nearby
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active && player.Distance(new Vector2(x, y) * 16) < 200f)
                {
                    return false;
                }
            }

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.Distance(new Vector2(x, y) * 16) < 200f)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up any managed resources
            }

            base.Dispose(disposing);
        }
    }
}
