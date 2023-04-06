using Newtonsoft.Json;

namespace BotPlugin
{
    public class BotConfig
    {
        [JsonProperty("botPrefix")]
        public string BotPrefix { get; set; } = "!bot ";

        [JsonProperty("botName")]
        public string BotName { get; set; } = "Bot";

        [JsonProperty("serverName")]
        public string ServerName { get; set; } = "Terraria Server";

        [JsonProperty("welcomeMessage")]
        public string WelcomeMessage { get; set; } = "Welcome to the server!";

        [JsonProperty("goodbyeMessage")]
        public string GoodbyeMessage { get; set; } = "Goodbye!";

        [JsonProperty("adminIds")]
        public List<string> AdminIds { get; set; } = new List<string>();
    }
}
