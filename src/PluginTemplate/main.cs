using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Newtonsoft.Json;

namespace SpecialEventsPlugin
{
    [ApiVersion(2, 1)]
    public class SpecialEventsPlugin : TerrariaPlugin
    {
        public override string Name => "SpecialEventsPlugin";
        public override string Author => "Same/AI";
        public override string Description => "Custom welcome/leave msg and special event access by player/group.";
        public override Version Version => new Version(1, 0, 0);

        private static string ConfigPath => Path.Combine(TShock.SavePath, "SpecialEventsConfig.json");
        private SpecialEventsConfig Config;

        public SpecialEventsPlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            LoadConfig();
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            Commands.ChatCommands.Add(new Command(CmdEvent, "event"));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            }
            base.Dispose(disposing);
        }

        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                Config = SpecialEventsConfig.Default();
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
            }
            else
            {
                Config = JsonConvert.DeserializeObject<SpecialEventsConfig>(File.ReadAllText(ConfigPath));
            }
        }

        private void OnJoin(JoinEventArgs args)
        {
            var plr = TShock.Players[args.Who];
            if (plr != null && plr.Active)
            {
                string msg = Config.WelcomeMessage.Replace("{player}", plr.Name);
                plr.SendMessage(msg, TSColor.Green);
            }
        }

        private void OnLeave(LeaveEventArgs args)
        {
            var plr = TShock.Players[args.Who];
            if (plr != null)
            {
                string msg = Config.LeaveMessage.Replace("{player}", plr.Name);
                TShock.Utils.Broadcast(msg, TSColor.Red);
            }
        }

        private void CmdEvent(CommandArgs args)
        {
            var plr = args.Player;
            if (IsAllowed(plr))
            {
                plr.SendSuccessMessage("Welcome to the special event!");
                // Place event code here
            }
            else
            {
                plr.SendErrorMessage("You are not allowed to join this event.");
            }
        }

        private bool IsAllowed(TSPlayer plr)
        {
            if (plr == null) return false;
            if (Config.AllowedPlayerNames.Contains(plr.Name))
                return true;
            foreach (var grp in Config.AllowedGroups)
            {
                if (plr.Group.Name.Equals(grp, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }

    public class SpecialEventsConfig
    {
        public string WelcomeMessage { get; set; } = "Welcome to the server, {player}!";
        public string LeaveMessage { get; set; } = "{player} has left. See you next time!";
        public List<string> AllowedPlayerNames { get; set; } = new List<string> { "YourNameHere" };
        public List<string> AllowedGroups { get; set; } = new List<string> { "admin", "eventer" };

        public static SpecialEventsConfig Default()
        {
            return new SpecialEventsConfig();
        }
    }
}
