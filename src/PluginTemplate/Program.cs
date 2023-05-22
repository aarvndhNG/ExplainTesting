using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace YourNamespace
{
    [ApiVersion(2, 1)]
    public class SocialFeatures : TerrariaPlugin
    {
        public override string Name => "SocialFeatures";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "YourName";
        public override string Description => "Social features for your TShock server.";

        private Dictionary<string, UserProfile> playerProfiles;
        private Dictionary<string, int> playerCurrency;
        private Config config;

        public SocialFeatures(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;
            NPC.NPCLoot += NPCLoot;
            Commands.ChatCommands.Add(new Command("profile", ProfileCommand, "profile"));
            Commands.ChatCommands.Add(new Command("friend", FriendCommand, "friend"));
            Commands.ChatCommands.Add(new Command("globalchat", GlobalChatCommand, "globalchat"));
            Commands.ChatCommands.Add(new Command("currency.add", AddCurrencyCommand, "addcurrency"));
            Commands.ChatCommands.Add(new Command("currency.remove", RemoveCurrencyCommand, "removecurrency"));
            Commands.ChatCommands.Add(new Command("currency.check", CheckCurrencyCommand, "checkcurrency"));
            Commands.ChatCommands.Add(new Command("currency.give", GiveCurrencyCommand, "givecurrency"));

            config = Config.Read();
            playerProfiles = new Dictionary<string, UserProfile>();
            playerCurrency = new Dictionary<string, int>();

            Config.Load();
        }

        private void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            string playerName = args.Player.Name;
            if (!playerProfiles.ContainsKey(playerName))
            {
                playerProfiles.Add(playerName, new UserProfile(playerName));
                playerCurrency.Add(playerName, config.InitialCurrency);
            }
        }

        private void NPCLoot(NpcLootEventArgs args)
        {
            int currencyAmount = 10; // Adjust the amount of currency given for killing an NPC here
            if (args.Npc != null && args.Npc.active && args.Npc.value > 0 && args.Npc.value >= currencyAmount)
            {
                string playerName = Main.player[args.Npc.target].name;
                if (playerProfiles.ContainsKey(playerName))
                {
                    playerCurrency[playerName] += currencyAmount;
                    TShock.Players[args.Npc.target].SendSuccessMessage($"You received {currencyAmount} {config.CurrencySymbol} for killing the NPC.");
                }
            }
        }

        private void ProfileCommand(CommandArgs args)
        {
            string playerName = args.Player.Name;

            if (args.Parameters.Count > 0)
            {
                playerName = args.Parameters[0];

                if (!playerProfiles.ContainsKey(playerName))
                {
                    args.Player.SendErrorMessage($"Profile for '{playerName}' does not exist.");
                    return;
                }
            }

            UserProfile profile = playerProfiles[playerName];

            args.Player.SendInfoMessage($"--- Profile for {playerName} ---");
            args.Player.SendInfoMessage($"Currency: {playerCurrency[playerName]} {config.CurrencySymbol}");
            args.Player.SendInfoMessage($"Friends: {string.Join(", ", friendLists[playerName])}");
            args.Player.SendInfoMessage($"Score: {playerScores[playerName]}");
        }

        private void FriendCommand(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid syntax! Usage: /friend <add|remove|list> [player]");
                return;
            }

            string subCommand = args.Parameters[0];

            switch (subCommand.ToLower())
            {
                case "add":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("Invalid syntax! Usage: /friend add <player>");
                        return;
                    }

                    string friendName = args.Parameters[1];

                    if (friendLists[args.Player.Name].Contains(friendName))
                    {
                        args.Player.SendErrorMessage($"'{friendName}' is already in your friend list.");
                        return;
                    }

                    friendLists[args.Player.Name].Add(friendName);
                    args.Player.SendSuccessMessage($"Added '{friendName}' to your friend list.");
                    break;

                case "remove":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("Invalid syntax! Usage: /friend remove <player>");
                        return;
                    }

                    string removedFriendName = args.Parameters[1];

                    if (!friendLists[args.Player.Name].Contains(removedFriendName))
                    {
                        args.Player.SendErrorMessage($"'{removedFriendName}' is not in your friend list.");
                        return;
                    }

                    friendLists[args.Player.Name].Remove(removedFriendName);
                    args.Player.SendSuccessMessage($"Removed '{removedFriendName}' from your friend list.");
                    break;

                case "list":
                    args.Player.SendInfoMessage($"--- Friend List of {args.Player.Name} ---");
                    args.Player.SendInfoMessage(string.Join(", ", friendLists[args.Player.Name]));
                    break;

                default:
                    args.Player.SendErrorMessage("Invalid subcommand! Available subcommands: add, remove, list");
                    break;
            }
        }

        private void GlobalChatCommand(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid syntax! Usage: /globalchat <join|leave>");
                return;
            }

            string subCommand = args.Parameters[0];

            switch (subCommand.ToLower())
            {
                case "join":
                    if (!args.Player.HasPermission("socialfeatures.globalchat"))
                    {
                        args.Player.SendErrorMessage("You don't have permission to join the global chat.");
                        return;
                    }

                    if (args.Player.GetGlobalData().ContainsKey("globalchat"))
                    {
                        args.Player.SendErrorMessage("You are already in the global chat.");
                        return;
                    }

                    args.Player.GetGlobalData()["globalchat"] = true;
                    args.Player.SendSuccessMessage("Joined the global chat.");
                    break;

                case "leave":
                    if (!args.Player.GetGlobalData().ContainsKey("globalchat"))
                    {
                        args.Player.SendErrorMessage("You are not in the global chat.");
                        return;
                    }

                    args.Player.GetGlobalData().Remove("globalchat");
                    args.Player.SendSuccessMessage("Left the global chat.");
                    break;

                default:
                    args.Player.SendErrorMessage("Invalid subcommand! Available subcommands: join, leave");
                    break;
            }
        }

        private void AddCurrencyCommand(CommandArgs args)
        {
            if (!args.Player.HasPermission("currency.add"))
            {
                args.Player.SendErrorMessage("You do not have permission to use this command.");
                return;
            }

            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /addcurrency <player> <amount>");
                return;
            }

            string playerName = args.Parameters[0];
            if (!playerCurrency.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage("Invalid player specified.");
                return;
            }

            int amount;
            if (!int.TryParse(args.Parameters[1], out amount) || amount <= 0)
            {
                args.Player.SendErrorMessage("Invalid amount specified.");
                return;
            }

            playerCurrency[playerName] += amount;
            args.Player.SendSuccessMessage($"Added {amount} {config.CurrencySymbol} to '{playerName}'.");
        }

        private void RemoveCurrencyCommand(CommandArgs args)
        {
            if (!args.Player.HasPermission("currency.remove"))
            {
                args.Player.SendErrorMessage("You do not have permission to use this command.");
                return;
            }

            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /removecurrency <player> <amount>");
                return;
            }

            string playerName = args.Parameters[0];
            if (!playerCurrency.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage("Invalid player specified.");
                return;
            }

            int amount;
            if (!int.TryParse(args.Parameters[1], out amount) || amount <= 0)
            {
                args.Player.SendErrorMessage("Invalid amount specified.");
                return;
            }

            if (playerCurrency[playerName] < amount)
            {
                args.Player.SendErrorMessage($"'{playerName}' does not have enough currency.");
                return;
            }

            playerCurrency[playerName] -= amount;
            args.Player.SendSuccessMessage($"Removed {amount} {config.CurrencySymbol} from '{playerName}'.");
        }

        private void CheckCurrencyCommand(CommandArgs args)
        {
            if (!args.Player.HasPermission("currency.check"))
            {
                args.Player.SendErrorMessage("You do not have permission to use this command.");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /checkcurrency <player>");
                return;
            }

            string playerName = args.Parameters[0];
            if (!playerCurrency.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage("Invalid player specified.");
                return;
            }

            int currency = playerCurrency[playerName];
            args.Player.SendInfoMessage($"'{playerName}' has {currency} {config.CurrencySymbol}.");
        }

        private void GiveCurrencyCommand(CommandArgs args)
        {
            if (!args.Player.HasPermission("currency.give"))
            {
                args.Player.SendErrorMessage("You do not have permission to use this command.");
                return;
            }

            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /givecurrency <player> <amount>");
                return;
            }

            string playerName = args.Parameters[0];
            if (!playerCurrency.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage("Invalid player specified.");
                return;
            }

            int amount;
            if (!int.TryParse(args.Parameters[1], out amount) || amount <= 0)
            {
                args.Player.SendErrorMessage("Invalid amount specified.");
                return;
            }

            if (amount > playerCurrency[args.Player.Name])
            {
                args.Player.SendErrorMessage("You do not have enough currency.");
                return;
            }

            playerCurrency[args.Player.Name] -= amount;
            playerCurrency[playerName] += amount;

            args.Player.SendSuccessMessage($"You gave {amount} {config.CurrencySymbol} to '{playerName}'.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;
                NPC.NPCLoot -= NPCLoot;
                Commands.ChatCommands.RemoveAll(c => c.HasAlias("profile"));
                Commands.ChatCommands.RemoveAll(c => c.HasAlias("friend"));
                Commands.ChatCommands.RemoveAll(c => c.HasAlias("globalchat"));
                Commands.ChatCommands.RemoveAll(c => c.HasAlias("addcurrency"));
                Commands.ChatCommands.RemoveAll(c => c.HasAlias("removecurrency"));
                Commands.ChatCommands.RemoveAll(c => c.HasAlias("checkcurrency"));
                Commands.ChatCommands.RemoveAll(c => c.HasAlias("givecurrency"));

                playerProfiles.Clear();
                playerCurrency.Clear();

                Config.Save();
            }
            base.Dispose(disposing);
        }
    }

    public class UserProfile
    {
        public string PlayerName { get; set; }

        // Add other profile properties here

        public UserProfile(string playerName)
        {
            PlayerName = playerName;
        }
    }

    public class Config
    {
        public int InitialCurrency { get; set; }
        public string CurrencySymbol { get; set; }

        public static string SavePath => Path.Combine(TShock.SavePath, "SocialFeaturesConfig.json");

        public static Config Read()
        {
            if (File.Exists(SavePath))
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(SavePath));
            }
            return new Config();
        }

        public static void Save()
        {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));
        }

        public static void Load()
        {
            if (!File.Exists(SavePath))
            {
                Config.Instance = new Config();
                Config.Save();
            }
            else
            {
                Config.Instance = Config.Read();
            }
        }

        public static Config Instance { get; set; }

        public Config()
        {
            InitialCurrency = 0;
            CurrencySymbol = "Currency";
        }
    }
}
