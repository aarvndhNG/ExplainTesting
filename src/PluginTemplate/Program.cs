using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace HousePlugin
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Config
    {
        [JsonProperty]
        public int Price { get; set; }
        [JsonProperty]
        public int HouseWidth { get; set; }
        [JsonProperty]
        public int HouseHeight { get; set; }
    }

    [ApiVersion(2, 1)]
    public class HousePlugin : TerrariaPlugin
    {
        private Config config;
        private int houseWidth;
        private int houseHeight;
        private int price;

        public HousePlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("house.build", BuildHouse, "buildhouse"));
            LoadConfig();
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SaveConfig();
            }
            base.Dispose(disposing);
        }

        private void LoadConfig()
        {
            var configFile = Path.Combine(TShock.SavePath, "housepluginconfig.json");
            if (File.Exists(configFile))
            {
                var json = File.ReadAllText(configFile);
                var newConfig = JsonConvert.DeserializeObject<Config>(json);
                price = newConfig.Price;
                houseWidth = newConfig.HouseWidth;
                houseHeight = newConfig.HouseHeight;
            }
            else
            {
                config = new Config
                {
                    Price = 1000,
                    HouseWidth = 10,
                    HouseHeight = 8
                };
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFile, json);
            }
        }

        private void SaveConfig()
        {
            config = new Config
            {
                Price = price,
                HouseWidth = houseWidth,
                HouseHeight = houseHeight
            };
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(Path.Combine(TShock.SavePath, "housepluginconfig.json"), json);
        }

        private void BuildHouse(CommandArgs args)
        {
            var player = args.Player;
            var width = houseWidth;
            var height = houseHeight;
            var x = player.TileX - width / 2;
            var y = player.TileY - height;
            var id = WorldGen.KillTile(x, y);
            if (id)
            {
                player.SendSuccessMessage("Building house...");
                WorldGen.BuildRoom(x, y, x + width, y + height);
                TShock.Utils.Broadcast($"{player.Name} has built a house!", Color.LimeGreen);
            }
        }

        public override Version Version => new Version(1, 0, 0);
        public override string Name => "HousePlugin";
        public override string Author => "Your Name";
        public override string Description => "Allows players to build houses with a single command.";

        public override object GetConfig()
        {
            return config;
        }

        public override void ReloadConfig(object config)
        {
            this.config = (Config)config;
            price = this.config.Price;
            houseWidth = this.config.HouseWidth;
            houseHeight = this.config.HouseHeight;
        }
    }
}
