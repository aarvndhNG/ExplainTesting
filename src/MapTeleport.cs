using System;
using System.IO;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace MapTeleport
{
    [ApiVersion(2, 1)]
    public class MapTeleport : TerrariaPlugin
    {
        public override string Author => "Quinci, updated by Maxthegreat99 + Kuz_ + Jules";
        public override string Description => "Teleports you to where you pinged on the map.";
        public override string Name => "MapTeleport";
        public override Version Version => new Version(1, 2, 0, 0);
        public MapTeleport(Main game) : base(game)
        {
        }

        private static class Permissions
        {
            public const string USE_MAPTP = "mapteleport.use";
            public const string TP_IN_BLOCKS = "mapteleport.solid";
            public const string AUTO_ENABLE_MAPTP = "mapteleport.autoenabled";
        }

        private const string DATA_KEY = "mapteleport";

        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
            Commands.ChatCommands.Add(new Command(Permissions.USE_MAPTP, ToggleMapTeleport, "mapteleport", "maptp")
            { HelpText = "Toggles map teleportation (Default: Disabled)" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreet);
            }
            base.Dispose(disposing);
        }

        private void OnGreet(GreetPlayerEventArgs args)
        {
            var player = TShock.Players[args.Who];

            if (player == null || !player.Active)
                return;

            player.SetData(DATA_KEY, player.HasPermission(Permissions.AUTO_ENABLE_MAPTP));
        }

        private void ToggleMapTeleport(CommandArgs args)
        {
            var player = args.Player;
            player.SetData<bool>(DATA_KEY, !player.GetData<bool>(DATA_KEY));
            player.SendSuccessMessage((player.GetData<bool>(DATA_KEY) ? "En" : "Dis") + "abled map teleport.");
        }

        private void SafeTeleport(TSPlayer player, float targetX, float targetY)
        {
            // Workaround for CheckSection NRE:
            // 1. Manually update position
            player.TPlayer.position.X = targetX;
            player.TPlayer.position.Y = targetY;

            // 2. Notify other players
            NetMessage.SendData((int)PacketTypes.PlayerUpdate, -1, player.Index, null, player.Index);

            // 3. Force the teleport packet directly to the player (Packet 65, style 1)
            NetMessage.SendData((int)PacketTypes.Teleport, player.Index, -1, null, 0, player.Index, targetX, targetY, 1);

            // 4. Safely check section. If the NetMessage update was handled, CheckSection might not be strictly needed,
            // but if we call it, we ensure map sections are pushed.
            try {
                Terraria.RemoteClient.CheckSection(player.Index, player.TPlayer.position);
            } catch (Exception ex) {
                TShock.Log.ConsoleError($"[MapTeleport] Suppressed error in CheckSection during teleport: {ex.Message}");
            }
        }

        private bool IsSafeTile(int x, int y)
        {
            // Modernized Tile Check for 1.4.5.6 (Tile is a struct, not a class, so no null check)
            var tile = Main.tile[x, y];
            return !tile.active() || (!Main.tileSolid[tile.type] && tile.liquid == 0);
        }

        private void OnGetData(GetDataEventArgs args)
        {
            using (MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
            using (BinaryReader reader = new BinaryReader(data))
            {
                if (args.MsgID == PacketTypes.LoadNetModule
                    && reader.ReadUInt16() == 2 // 2 is Ping
                    && TShock.Players[args.Msg.whoAmI].GetData<bool>(DATA_KEY)
                    && TShock.Players[args.Msg.whoAmI].HasPermission(Permissions.USE_MAPTP))
                {
                    TSPlayer player = TShock.Players[args.Msg.whoAmI];

                    float pingX = reader.ReadSingle();
                    float pingY = reader.ReadSingle();

                    // Convert to tiles
                    int X = Math.Min(Main.maxTilesX - 1, Math.Max(0, (int)(pingX / 16f)));
                    int Y = Math.Min(Main.maxTilesY - 1, Math.Max(0, (int)(pingY / 16f)));

                    if ((X == Main.maxTilesX - 1 || X == 0) && (Y == Main.maxTilesY - 1 || Y == 0))
                        return;

                    if (player.HasPermission(Permissions.TP_IN_BLOCKS))
                    {
                        SafeTeleport(player, pingX, pingY);
                        player.SendSuccessMessage($"Teleported to map ping ({X}, {Y}).");
                    }
                    else
                    {
                        bool canTeleport = IsSafeTile(X, Y)
                                        && IsSafeTile(X + 1, Y)
                                        && IsSafeTile(X + 1, Y + 1)
                                        && IsSafeTile(X, Y + 1)
                                        && IsSafeTile(X + 1, Y + 2)
                                        && IsSafeTile(X, Y + 2);

                        if (canTeleport)
                        {
                            SafeTeleport(player, pingX, pingY);
                            player.SendSuccessMessage($"Teleported to map ping ({X}, {Y}).");
                        }
                        else
                        {
                            player.SendErrorMessage("You do not have permission to teleport into solid tiles.");
                        }
                    }

                    args.Handled = true;
                    return;
                }
            }
        }
    }
}
