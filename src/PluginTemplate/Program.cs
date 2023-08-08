using System;
using System.IO;
using System.Net.Http;
using Terraria;
using TShockAPI;
using TShockAPI.Hooks;
using TerrariaApi.Server;
using Newtonsoft.Json.Linq;

namespace ServerStatusPlugin
{
    [ApiVersion(2, 1)]
    public class ServerStatusPlugin : TerrariaPlugin
    {
        private ServerStatusConfig _config;

        public override string Author => "Your Name";
        public override string Description => "Displays server status from your website.";
        public override string Name => "ServerStatusPlugin";
        public override Version Version => new Version(1, 0, 0);

        public ServerStatusPlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += OnReload;
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;

            SetupConfig();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TShockAPI.Hooks.GeneralHooks.ReloadEvent -= OnReload;
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;
            }
            base.Dispose(disposing);
        }

        private void OnReload(ReloadEventArgs args)
        {
            SetupConfig();
        }

        private void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            CheckServerStatus(args.Player);
        }

        private async void CheckServerStatus(TSPlayer player)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync(_config.ApiUrl);
                    var jsonResponse = JObject.Parse(response);
                    var serverStatus = jsonResponse["status"].ToString();

                    string statusMessage = serverStatus == "online"
                        ? "Server Status: Online"
                        : "Server Status: Offline";

                    TShock.Utils.Broadcast(statusMessage, Color.Green);
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                }
            }
        }

        private void SetupConfig()
        {
            string configPath = Path.Combine(TShock.SavePath, "ServerStatusConfig.json");

            if (!File.Exists(configPath))
            {
                _config = new ServerStatusConfig();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
            else
            {
                _config = JsonConvert.DeserializeObject<ServerStatusConfig>(File.ReadAllText(configPath));
            }

            if (string.IsNullOrWhiteSpace(_config.GeneratedUrl))
            {
                _config.GeneratedUrl = GenerateServerUrl();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
        }

        private string GenerateServerUrl()
        {
            // Logic to generate the URL based on your needs
            return "https://your-terraria-server-url";
        }
    }
}
