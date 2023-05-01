using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace PvPPlugin
{
    [ApiVersion(2, 1)]
    public class PvPPlugin : TerrariaPlugin
    {
        private Dictionary<string, PvPArena> arenas;
        private Config config;

        public override string Name => "PvPPlugin";
        public override string Author => "Your Name";
        public override string Description => "Provides PvP arenas for players to fight in.";
        public override Version Version => new Version(1, 0, 0);

        public PvPPlugin(Main game) : base(game)
        {
            arenas = new Dictionary<string, PvPArena>();
        }

        public override void Initialize()
        {
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;

            string configPath = Path.Combine(TShock.SavePath, "PvPArena.json");
            if (File.Exists(configPath))
            {
                string configJson = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<Config>(configJson);
                foreach (var arenaConfig in config.Arenas)
                {
                    var arena = new PvPArena(arenaConfig.Name, arenaConfig.RedSpawn, arenaConfig.BlueSpawn, arenaConfig.Bounds);
                    arenas.Add(arenaConfig.Name, arena);
                }
            }
            else
            {
                config = new Config();
                string configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, configJson);
            }

            Commands.ChatCommands.Add(new Command("pvparena.create", CreateArena, "createarena")
            {
                HelpText = "Creates a new PvP arena with the specified name, red and blue team spawn points, and arena boundaries."
            });

            Commands.ChatCommands.Add(new Command("pvparena.delete", DeleteArena, "deletearena")
            {
                HelpText = "Deletes the PvP arena with the specified name."
            });

            Commands.ChatCommands.Add(new Command("pvparena.list", ListArenas, "listarenas")
            {
                HelpText = "Lists all available PvP arenas."
            });

            Commands.ChatCommands.Add(new Command("pvparena.join", JoinArena, "joinarena")
            {
                HelpText = "Joins the specified PvP arena."
            });

            Commands.ChatCommands.Add(new Command("pvparena.leave", LeaveArena, "leavearena")
            {
                HelpText = "Leaves the current PvP arena."
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;
            }
            base.Dispose(disposing);
        }

        private void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            var player = TShock.Players[args.PlayerId];
            if (player != null)
            {
                player.SetData("PvPArena", "");
            }
        }

        private void SaveConfig()
        {
            string configPath = Path.Combine(TShock.SavePath, "PvPArena.json");
            string configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, configJson);
        }

        [Command("pvparena setjoindelay")]
        public void SetArenaJoinDelay(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /pvparena setjoindelay <delay>");
                return;
            }

            if (!int.TryParse(args.Parameters[0], out int delay))
            {
                args.Player.SendErrorMessage("Invalid delay value!");
                return;
            }

            config.ArenaJoinDelay = delay;
            SaveConfig();
            args.Player.SendSuccessMessage($"Arena join delay set to {delay} seconds.");
        }

        private void CreateArena(CommandArgs args)
        {
            if (args.Parameters.Count < 5)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /createarena <name> <redX> <redY> <blueX> <blueY> <left> <top> <width> <height>");
                return;
            }

            string name = args.Parameters[0];
            if (arenas.ContainsKey(name))
            {
                args.Player.SendErrorMessage($"An arena with the name '{name}' already exists!");
                return;
            }

            if (!float.TryParse(args.Parameters[1], out float redX) || !float.TryParse(args.Parameters[2], out float redY) ||
                !float.TryParse(args.Parameters[3], out float blueX) || !float.TryParse(args.Parameters[4], out float blueY) ||
                !int.TryParse(args.Parameters[5], out int left) || !int.TryParse(args.Parameters[6], out int top) ||
                !int.TryParse(args.Parameters[7], out int width) || !int.TryParse(args.Parameters[8], out int height))
            {
                args.Player.SendErrorMessage("Invalid parameter values!");
                return;
            }

            var redSpawn = new Vector2(redX, redY);
            var blueSpawn = new Vector2(blueX, blueY);
            var bounds = new Rectangle(left, top, width, height);

            var arena = new PvPArena(name, redSpawn, blueSpawn, bounds);
            arenas.Add(name, arena);

            var arenaConfig = new ArenaConfig
            {
                Name = name,
                RedSpawn = redSpawn,
                BlueSpawn = blueSpawn,
                Bounds = bounds
            };
            config.Arenas.Add(arenaConfig);
            SaveConfig();

            args.Player.SendSuccessMessage($"PvP arena '{name}' created successfully!");
        }

        private void DeleteArena(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /deletearena <name>");
                return;
            }

            string name = args.Parameters[0];
            if (!arenas.ContainsKey(name))
            {
                args.Player.SendErrorMessage($"An arena with the name '{name}' does not exist!");
                return;
            }

            var arena = arenas[name];
            arena.Disable();
            arenas.Remove(name);

            var arenaConfig = config.Arenas.Find(a => a.Name == name);
            config.Arenas.Remove(arenaConfig);
            SaveConfig();

            args.Player.SendSuccessMessage($"PvP arena '{name}' deleted successfully!");
        }

        private void ListArenas(CommandArgs args)
        {
            if (arenas.Count == 0)
            {
                args.Player.SendInfoMessage("There are no PvP arenas available.");
                return;
            }

            args.Player.SendInfoMessage("Available PvP arenas:");
            foreach (var arena in arenas.Values)
            {
                args.Player.SendInfoMessage($"- {arena.Name}");
            }
        }

        private void JoinArena(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /joinarena <name>");
                return;
            }

            string name = args.Parameters[0];
            if (!arenas.ContainsKey(name))
            {
                args.Player.SendErrorMessage($"An arena with the name '{name}' does not exist!");
                return;
            }

            var arena = arenas[name];
            if (!arena.Bounds.Contains((int)args.Player.X, (int)args.Player.Y))
            {
                args.Player.SendErrorMessage($"You must be inside the {name} arena to join!");
                return;
            }

            string currentArena = args.Player.GetData<string>("PvPArena");
            if (!string.IsNullOrEmpty(currentArena))
            {
                args.Player.SendErrorMessage($"You are already in the {currentArena} arena!");
                return;
            }

            args.Player.SetData("PvPArena", name);
            args.Player.SendSuccessMessage($"You have joined the {name} arena!");
        }

        private void LeaveArena(CommandArgs args)
        {
            string currentArena = args.Player.GetData<string>("PvPArena");
            if (string.IsNullOrEmpty(currentArena))
            {
                args.Player.SendErrorMessage("You are not currently in a PvP arena!");
                return;
            }

            args.Player.SetData("PvPArena", "");
            args.Player.SendSuccessMessage($"You have left the {currentArena} arena!");
        }

        private class Config
        {
            public List<ArenaConfig> Arenas { get; set; } = new List<ArenaConfig>();
            public int ArenaJoinDelay { get; set; } = 5;
        }

        private class ArenaConfig
        {
            public string Name { get; set; }
            public Vector2 RedSpawn { get; set; }
            public Vector2 BlueSpawn { get; set; }
            public Rectangle Bounds { get; set; }
        }
    }

    public class PvPArena
    {
        public string Name { get; }
        public Vector2 RedSpawn { get; }
        public Vector2 BlueSpawn { get; }
        public Rectangle Bounds { get; }

        public PvPArena(string name, Vector2 redSpawn, Vector2 blueSpawn, Rectangle bounds)
        {
            Name = name;
            RedSpawn = redSpawn;
            BlueSpawn = blueSpawn;
            Bounds = bounds;
        }

        public void Enable()
        {
            TShock.Utils.Broadcast($"The {Name} arena is now open for PvP battles!", Color.Yellow);
        }

        public void Disable()
        {
            TShock.Utils.Broadcast($"The {Name} arena is now closed for PvP battles.", Color.Yellow);
        }
    }
}
