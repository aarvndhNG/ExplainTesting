using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace ZombieModePlus
{
    [ApiVersion(2, 1)]
    public class ZombieModePlus : TerrariaPlugin
    {
        public static readonly string PluginName = "ZombieModePlus";

        public static Dictionary<int, Room> Rooms { get; set; }
        public static Dictionary<string, Pack> Packs { get; set; }
        public static bool GameInProgress { get; set; }
        public static Config Configuration { get; set; }

        public override string Name => PluginName;
        public override string Author => "Bard";
        public override Version Version => new Version(1, 1, 0);

        public override void Initialize()
        {
            Rooms = new Dictionary<int, Room>();
            Packs = new Dictionary<string, Pack>();
            GameInProgress = false;
            Configuration = LoadConfig();

            // Register chat commands
            TShockAPI.Commands.ChatCommands.Add(new Command("zma.admin", AdminCommands, "Zombie Mode admin commands"));
            TShockAPI.Commands.ChatCommands.Add(new Command("zm", PlayerCommands, "Zombie Mode player commands"));
        }

        private Config LoadConfig()
        {
            string filePath = Path.Combine(TShockAPI.GetDataDirectory(), PluginName + ".json");

            if (!File.Exists(filePath))
            {
                // Create default configuration file
                Config config = new Config();
                File.WriteAllText(filePath, JsonConvert.SerializeObject(config, Formatting.Indented));

                return config;
            }

            // Load configuration from file
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filePath));

            return config;
        }

        private void AdminCommands(CommandArgs args)
        {
            // Delegate command execution to separate class
            Commands.AdminCommands.HandleCommand(args);
        }

        private void PlayerCommands(CommandArgs args)
        {
            // Delegate command execution to separate class
            Commands.PlayerCommands.HandleCommand(args);
        }
    }
}
