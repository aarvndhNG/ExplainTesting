using System;
using System.IO;
using System.IO.Streams;
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
            Commands.ChatCommands.Add(new Command( Permissions.USE_MAPTP, ToggleMapTeleport, "mapteleport", "maptp")
            { HelpText = "Toggles map teleportation (Default: Disabled)"});
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
        private void OnGreet(GreetPlayerEventArgs args) {
            var player = TShock.Players[args.Who];

			if (player == null || !player.Active)
				return;


            player.SetData(DATA_KEY, player.HasPermission(Permissions.AUTO_ENABLE_MAPTP));
        }
        private void ToggleMapTeleport(CommandArgs args) {

            var player = args.Player;

            player.SetData<bool>(DATA_KEY,!player.GetData<bool>(DATA_KEY));

            player.SendSuccessMessage((player.GetData<bool>(DATA_KEY) ? "En" : "Dis") + "abled map teleport.");
        }

        private bool IsSafeTile(int x, int y)
        {
            var tile = Main.tile[x, y];
            return !tile.active() || (!Main.tileSolid[tile.type] && tile.liquid == 0);
        }

        private void OnGetData(GetDataEventArgs args) {

            using (MemoryStream data = new MemoryStream(args.Msg.readBuffer, 3, args.Length - 1))
            {
                if (args.MsgID == PacketTypes.LoadNetModule
                    && data.ReadByte() == 2
                    && TShock.Players[args.Msg.whoAmI].GetData<bool>(DATA_KEY)
                    && TShock.Players[args.Msg.whoAmI].HasPermission(Permissions.USE_MAPTP))
                {
                    TSPlayer player = TShock.Players[args.Msg.whoAmI];

                    data.Position++;

                    int X = Math.Min(Main.maxTilesX, Math.Max(0, (int)data.ReadSingle()));
                    int Y = Math.Min(Main.maxTilesY, Math.Max(0, (int)data.ReadSingle()));


                    if ((X == Main.maxTilesX || X == 0)
                       && (Y == Main.maxTilesY || Y == 0))
                        return;

                    if (player.HasPermission(Permissions.TP_IN_BLOCKS))
                    {
                        player.Teleport(X * 16, Y * 16);
                        player.SendSuccessMessage($"Teleported to ({X}, {Y})");
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
                            player.Teleport(X * 16, Y * 16);
                            player.SendSuccessMessage($"Teleported to ({X}, {Y})");
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
