using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace HelperBot
{
    public class ConfigFile
    {
        // Config variables here:
        public string BotName { get; set; } = "HelperBot";
        public string BaseCommandName { get; set; } = "hb";
        public int[] BotColor { get; set; } = { 69, 201, 210 };
        public bool EnableBotCommands { get; set; } = true;

        public class QA
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }

        public List<QA> QuestionsAndAnswers { get; set; } = new List<QA>();
        // End of config variables

        public static ConfigFile Read(string path)
        {
            if (!File.Exists(path))
            {
                ConfigFile config = new ConfigFile();

                config.QuestionsAndAnswers.Add(new QA()
                {
                    Question = @"How (can i|to) register(\?|)",
                    Answer = $"Use {TShock.Config.CommandSpecifier}register <password>"
                });

                config.QuestionsAndAnswers.Add(new QA()
                {
                    Question = @"How i go to the spawn(\?|)",
                    Answer = $"Use {TShock.Config.CommandSpecifier}spawn"
                });

                config.QuestionsAndAnswers.Add(new QA()
                {
                    Question = @"who (made|created|coded) the helperbot(\?|)",
                    Answer = "Ryozuki, you can find his website here: ryobyte.com"
                });

                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
                return config;
            }
            return JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(path));
        }
    }
}
