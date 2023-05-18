using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace HelperBot
{
    [ApiVersion(2, 1)]
    public class HelperBot : TerrariaPlugin
    {
        #region Plugin Info
        public override string Name => "HelperBot";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public override string Author => "Ryozuki";
        public override string Description => "A bot with multiple utilities, such as stats gathering and a Q&A system.";
        #endregion

        public ConfigFile Config { get; set; } = new ConfigFile();
        public Regex ReminderRegex { get; set; } = new Regex(@"remind me to ([\w+ ]+) in (\d+) (minutes|mins|hours|seconds|secs|days)");

        public HelperBot(Main game) : base(game)
        {
        }

        private void LoadConfig()
        {
            string path = Path.Combine(TShock.SavePath, "HelperBot.json");
            Config = ConfigFile.Read(path);
        }

        public override void Initialize()
        {
            LoadConfig();
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerChat.Register(this, OnChat, -666);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            }
            base.Dispose(disposing);
        }

        #region Hooks
        private void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("helperbot.cmds", CommandHandler, Config.BaseCommandName)
            {
                HelpText = $"Usage: {TShock.Config.CommandSpecifier}{Config.BaseCommandName} <command>"
            });
        }

        private void OnChat(ServerChatEventArgs e)
        {
            string msg = e.Text;

            var ply = TShock.Players[e.Who];

            if (ply == null)
                return;

            msg = msg.ToLower();

            if (Config.QuestionsAndAnswers.Any(x => Regex.Match(msg.ToLower(), x.Question.ToLower()).Success))
            {
                var answer = Config.QuestionsAndAnswers.Find(x => Regex.Match(msg.ToLower(), x.Question.ToLower()).Success)?.Answer;

                TShockAPI.Utils.Broadcast($"{Config.BotName}: {answer}", (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
            }
            else if (msg.Contains("how much is"))
            {
                int index = msg.LastIndexOf('s');
                string op = msg.Substring(index + 1);
                op = op.Replace("?", "");

                try
                {
                    double result = Convert.ToDouble(new DataTable().Compute(op, null));
                    TShockAPI.Utils.Broadcast($"{Config.BotName}: The result is: {result}", (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
                }
                catch (System.Data.EvaluateException)
                {
                    // invalid operation
                }
                catch (System.Data.SyntaxErrorException)
                {
                    // invalid operation
                }
            }
            else if (ReminderRegex.IsMatch(msg) && ply.HasPermission("helperbot.remind"))
            {
                Match m = ReminderRegex.Match(msg);

                string what = m.Groups[1].Value;
                float time = float.Parse(m.Groups[2].Value);
                string time_unit = m.Groups[3].Value;

                if (time_unit == "mins")
                    time_unit = "minutes";
                if (time_unit == "secs")
                    time_unit = "seconds";

                TShockAPI.Utils.Broadcast($"{Config.BotName}: {ply.Name} I'll remind you to {what} in {time} {time_unit}", (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);

                if (time_unit == "days")
                    time *= 24 * 3600 * 1000;
                if (time_unit == "hours")
                    time *= 3600 * 1000;
                else if (time_unit == "minutes")
                    time *= 60 * 1000;
                else if (time_unit == "seconds")
                    time *= 1000;

                Timer timer = new Timer(time);
                timer.Elapsed += (sender, ee) => TShockAPI.Utils.Broadcast($"{Config.BotName}: {ply.Name} remember to {what}", (byte)Config.BotColor[0], (byte)Config.BotColor[1], (byte)Config.BotColor[2]);
                timer.AutoReset = false;
                timer.Start();
            }
        }
        #endregion

        #region Commands
        private void CommandHandler(CommandArgs e)
        {
            if (!Config.EnableBotCommands)
                return;

            var ply = e.Player;
            var cmd = "";

            if (e.Parameters.Count != 0)
            {
                cmd = e.Parameters[0];
            }

            var args = e.Parameters.Skip(1).ToArray();

            switch (cmd.ToLower())
            {
                case "register":
                    if (args.Length < 1)
                    {
                        ply.SendErrorMessage("Usage: {0}register <password>", TShock.Config.CommandSpecifier);
                        return;
                    }

                    if (ply.User != null)
                    {
                        ply.SendErrorMessage("You are already registered.");
                        return;
                    }

                    var registerPassword = args[0];

                    if (ply.Register(registerPassword))
                    {
                        ply.SendSuccessMessage("You have successfully registered.");
                    }
                    else
                    {
                        ply.SendErrorMessage("Registration failed.");
                    }
                    break;

                case "spawn":
                    ply.Teleport(TShock.Players.FirstOrDefault(p => p?.IsLoggedIn == true)?.TileX ?? 0 * 16, TShock.Players.FirstOrDefault(p => p?.IsLoggedIn == true)?.TileY ?? 0 * 16);
                    break;

                default:
                    ply.SendErrorMessage("Invalid command. Type {0}help {1} for a list of available commands.", TShock.Config.CommandSpecifier, Config.BaseCommandName);
                    break;
            }
        }
        #endregion
    }
}
