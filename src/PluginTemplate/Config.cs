using Newtonsoft.Json;
using System.IO;

namespace WinterMinigame
{
    public class Config
    {
        public int GameDuration { get; set; }
        public Position StartingPosition { get; set; }

        public static Config Read()
        {
            var filePath = Path.Combine(TShock.SavePath, "WinterMinigame.json");
            if (File.Exists(filePath))
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(filePath));
            }

            var config = new Config
            {
                GameDuration = 5, // default game duration in minutes
                StartingPosition = new Position { X = 0, Y = 0 } // default starting position
            };

            config.Write(); // create default config file
            return config;
        }

        public void Write()
        {
            var filePath = Path.Combine(TShock.SavePath, "WinterMinigame.json");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
