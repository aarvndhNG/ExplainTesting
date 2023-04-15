using System;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace SkySpawn
{
    [ApiVersion(2, 1)]
    public class SkySpawn : TerrariaPlugin
    {
        public override string Name => "SkySpawn";
        public override string Author => "Your Name Here";
        public override string Description => "Allows players to spawn items from the sky.";
        public override Version Version => new Version(1, 0, 0);

        public SkySpawn(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("skyspawn.spawnitem", SkySpawnItem, "skyitem"));
        }

        private void SkySpawnItem(CommandArgs args)
        {
            if (!CanSpawnItem(args.Player))
            {
                args.Player.SendErrorMessage("You do not have permission to use this command.");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /skyitem <item name or ID> [stack size]");
                return;
            }

            string itemName = String.Join(" ", args.Parameters.GetRange(0, args.Parameters.Count - 1));
            int stackSize;

            if (!int.TryParse(args.Parameters[args.Parameters.Count - 1], out stackSize))
            {
                stackSize = 1;
            }

            if (stackSize > 999)
            {
                args.Player.SendErrorMessage("You cannot spawn more than 999 items at once.");
                return;
            }

            var item = TShock.Utils.GetItemByIdOrName(itemName);

            if (item == null || item.type <= 0 || item.type >= Main.maxItemTypes)
            {
                args.Player.SendErrorMessage("Invalid item name or ID!");
                return;
            }

            var x = (int)(args.Player.TPlayer.position.X + (args.Player.TPlayer.width / 2)) / 16;
            var y = (int)(args.Player.TPlayer.position.Y + (args.Player.TPlayer.height / 2)) / 16;

            var itemNetID = (short)item.type;
            var stack = (byte)stackSize;
            var prefix = 0;
            var itemID = Item.NewItem(x, y, 0, 0, itemNetID, stack, true, prefix, false);
            NetMessage.SendData(21, -1, -1, null, itemID);
            TShock.Log.ConsoleInfo("{0} spawned {1} ({2}) {3} time(s) from the sky.", args.Player.Name, item.Name, item.type, stackSize);
        }

        private bool CanSpawnItem(TSPlayer player)
        {
            return player.RealPlayer || player.Group.HasPermission("skyspawn.spawnitem.owner");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Cleanup
            }
            base.Dispose(disposing);
        }
    }
}
