using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Paintball
{
    [ApiVersion(2, 1)]
    public class PaintballPlugin : TerrariaPlugin
    {
        public override string Name => "Paintball";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "YourName";
        public override string Description => "A fast-paced game where players shoot paintballs at each other in an arena, trying to eliminate opponents and be the last one standing.";

        private List<string> playersInGame;
        private Dictionary<string, DateTime> lastShootTimes;

        private int paintballItemId;
        private int paintballProjectileType;
        private float paintballSpeed;

        public PaintballPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            playersInGame = new List<string>();
            lastShootTimes = new Dictionary<string, DateTime>();

            LoadConfig();

            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            Commands.ChatCommands.Add(new Command("paintball", PaintballCommand, "paintball"));
        }

        public void DeInitialize()
        {
            playersInGame.Clear();
            lastShootTimes.Clear();

            SaveConfig();
        }


        private void LoadConfig()
        {
            var configPath = Path.Combine(TShock.SavePath, "paintball_config.json");

            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<Config>(configText);

                if (config != null)
                {
                    paintballItemId = config.PaintballItemId;
                    paintballProjectileType = config.PaintballProjectileType;
                    paintballSpeed = config.PaintballSpeed;
                }
                else
                {
                    TShock.Log.Error("Failed to parse paintball configuration file. Using default values.");
                    SetDefaultConfig();
                }
            }
            else
            {
                TShock.Log.Info("Paintball configuration file not found. Using default values.");
                SetDefaultConfig();
            }
        }

        private void SaveConfig()
        {
            var configPath = Path.Combine(TShock.SavePath, "paintball_config.json");
            var config = new Config
            {
                PaintballItemId = paintballItemId,
                PaintballProjectileType = paintballProjectileType,
                PaintballSpeed = paintballSpeed
            };

            var configText = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, configText);
        }

        private void SetDefaultConfig()
        {
            paintballItemId = 1234; // Default paintball item ID
            paintballProjectileType = 5678; // Default paintball projectile type
            paintballSpeed = 10f; // Default paintball speed
        }

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.PlayerUpdate)
            {
                var playerIndex = args.Msg.whoAmI;
                var player = TShock.Players[playerIndex];

                if (player == null || !playersInGame.Contains(player.Name))
                    return;

                var velX = BitConverter.ToSingle(args.Msg.readBuffer, args.Index + 3);
                var velY = BitConverter.ToSingle(args.Msg.readBuffer, args.Index + 7);

                if (Math.Abs(velX) > 0.01f || Math.Abs(velY) > 0.01f)
                {
                    // Player is moving, check if they are trying to shoot a paintball
                    if (DateTime.Now - lastShootTimes[player.Name] > TimeSpan.FromSeconds(0.5))
                    {
                        var paintballItem = new Item();
                        paintballItem.SetDefaults(paintballItemId);
                        paintballItem.stack = 1;

                        var paintballSpeed = 10.0;

                        int paintballProjectileType = ProjectileID.Paintball;

                        NetMessage.SendData(27, -1, -1, null, paintball);
                        lastShootTimes[player.Name] = DateTime.Now;
                    }
                }
            }
        }

        private void PaintballCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /paintball <join|leave>");
                return;
            }

            var subCommand = args.Parameters[0];

            switch (subCommand.ToLower())
            {
                case "join":
                    if (!playersInGame.Contains(args.Player.Name))
                    {
                        playersInGame.Add(args.Player.Name);
                        lastShootTimes[args.Player.Name] = DateTime.Now;
                        args.Player.SendSuccessMessage("You joined the paintball game.");
                    }
                    else
                    {
                        args.Player.SendInfoMessage("You are already in the paintball game.");
                    }
                    break;

                case "leave":
                    if (playersInGame.Contains(args.Player.Name))
                    {
                        playersInGame.Remove(args.Player.Name);
                        lastShootTimes.Remove(args.Player.Name);
                        args.Player.SendSuccessMessage("You left the paintball game.");
                    }
                    else
                    {
                        args.Player.SendInfoMessage("You are not currently in the paintball game.");
                    }
                    break;

                default:
                    args.Player.SendErrorMessage("Invalid subcommand! Available subcommands: join, leave");
                    break;
            }
        }
    }

    public class Config
    {
        public int PaintballItemId { get; set; }
        public int PaintballProjectileType { get; set; }
        public float PaintballSpeed { get; set; }
    }
}
