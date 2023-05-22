using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace SocialFeatures
{
    [ApiVersion(2, 1)]
    public class SocialFeaturesPlugin : TerrariaPlugin
    {
        private Dictionary<string, UserProfile> playerProfiles;
        private Dictionary<string, List<string>> friendLists;
        private Dictionary<string, int> playerScores;
        private Dictionary<string, int> playerCurrency;

        public override string Name => "SocialFeatures";
        public override string Author => "YourName";
        public override string Description => "Adds social features to your TShock server.";
        public override Version Version => new Version(1, 0, 0);

        private Config config;

        public SocialFeaturesPlugin(Main game) : base(game)
        {
            playerProfiles = new Dictionary<string, UserProfile>();
            friendLists = new Dictionary<string, List<string>>();
            playerScores = new Dictionary<string, int>();
            playerCurrency = new Dictionary<string, int>();

            config = new Config();
        }

        public override void Initialize()
        {
            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
            NPC.NPCLoot += NPCLoot;
            Commands.ChatCommands.Add(new Command("profile", ProfileCommand, "profile"));
            Commands.ChatCommands.Add(new Command("friend", FriendCommand, "friend"));
            Commands.ChatCommands.Add(new Command("globalchat", GlobalChatCommand, "globalchat"));
            Commands.ChatCommands.Add(new Command("currency.admin", AddCurrencyCommand, "addcurrency"));
            Commands.ChatCommands.Add(new Command("currency.admin", RemoveCurrencyCommand, "removecurrency"));
            Commands.ChatCommands.Add(new Command("currency", CheckCurrencyCommand, "checkcurrency"));
            Commands.ChatCommands.Add(new Command("currency.admin", GiveCurrencyCommand, "givecurrency"));
            Config.Load();
        }

        private void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            string playerName = args.Player.Name;

            // Create player profile if it doesn't exist
            if (!playerProfiles.ContainsKey(playerName))
            {
                playerProfiles[playerName] = new UserProfile(playerName);
                friendLists[playerName] = new List<string>();
                playerScores[playerName] = 0;
                playerCurrency[playerName] = config.InitialCurrency; // Set initial currency balance from config
            }
        }

        private void OnNetGreetPlayer(GreetPlayerEventArgs args)
        {
            string playerName = TShock.Players[args.Who].Name;

            if (playerProfiles.ContainsKey(playerName))
            {
                args.Handled = true;
                TShock.Players[args.Who].SendMessage($"Welcome back, {playerName}!", Color.Yellow);
            }
        }

        private void NPCLoot(NpcLootEventArgs args)
        {
            // Check if the killer is a player and has a valid profile
            if (args.Player != null && playerProfiles.ContainsKey(args.Player.Name))
            {
                int currencyReward = 0;

                // Determine the currency reward based on the NPC's type or ID
                switch (args.Npc.type)
                {
                    // Example: Reward 10 currency for killing a slime
                    case NPCID.BlueSlime:
                    case NPCID.GreenSlime:
                    case NPCID.RedSlime:
                        currencyReward = 10;
                        break;

                    // Example: Reward 50 currency for killing a boss
                    case NPCID.EyeofCthulhu:
                    case NPCID.EaterofWorldsHead:
                    case NPCID.BrainofCthulhu:
                        currencyReward = 50;
                        break;

                    // Add more cases for other NPCs as needed

                    default:
                        // No currency reward for other NPCs
                        break;
                }

                // Add the currency reward to the player's balance
                if (currencyReward > 0)
                {
                    string playerName = args.Player.Name;
                    playerCurrency[playerName] += currencyReward;
                    args.Player.SendSuccessMessage($"You received {currencyReward} currency for defeating the {args.Npc.FullName}.");
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
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Usage: /addcurrency <player> <amount>");
                return;
            }

            string playerName = args.Parameters[0];
            int amount;

            if (!int.TryParse(args.Parameters[1], out amount))
            {
                args.Player.SendErrorMessage("Invalid amount specified.");
                return;
            }

            if (!playerProfiles.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage($"Profile for '{playerName}' does not exist.");
                return;
            }

            playerCurrency[playerName] += amount;
            args.Player.SendSuccessMessage($"Added {amount} {config.CurrencySymbol} to '{playerName}'s balance.");
        }

        private void RemoveCurrencyCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Usage: /removecurrency <player> <amount>");
                return;
            }

            string playerName = args.Parameters[0];
            int amount;

            if (!int.TryParse(args.Parameters[1], out amount))
            {
                args.Player.SendErrorMessage("Invalid amount specified.");
                return;
            }

            if (!playerProfiles.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage($"Profile for '{playerName}' does not exist.");
                return;
            }

            if (playerCurrency[playerName] < amount)
            {
                args.Player.SendErrorMessage($"'{playerName}' does not have enough currency.");
                return;
            }

            playerCurrency[playerName] -= amount;
            args.Player.SendSuccessMessage($"Removed {amount} {config.CurrencySymbol} from '{playerName}'s balance.");
        }

        private void CheckCurrencyCommand(CommandArgs args)
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

            args.Player.SendInfoMessage($"--- Currency Balance for {playerName} ---");
            args.Player.SendInfoMessage($"{playerCurrency[playerName]} {config.CurrencySymbol}");
        }

        private void GiveCurrencyCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Usage: /givecurrency <player> <amount>");
                return;
            }

            string playerName = args.Parameters[0];
            int amount;

            if (!int.TryParse(args.Parameters[1], out amount))
            {
                args.Player.SendErrorMessage("Invalid amount specified.");
                return;
            }

            if (!playerProfiles.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage($"Profile for '{playerName}' does not exist.");
                return;
            }

            if (amount > playerCurrency[args.Player.Name])
            {
                args.Player.SendErrorMessage("You don't have enough currency.");
                return;
            }

            playerCurrency[args.Player.Name] -= amount;
            playerCurrency[playerName] += amount;
            args.Player.SendSuccessMessage($"Gave {amount} {config.CurrencySymbol} to '{playerName}'.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnNetGreetPlayer);
                NPC.NPCLoot -= NPCLoot;
                Commands.ChatCommands.RemoveAll(x => x.HasAlias("profile"));
                Commands.ChatCommands.RemoveAll(x => x.HasAlias("friend"));
                Commands.ChatCommands.RemoveAll(x => x.HasAlias("globalchat"));
                Commands.ChatCommands.RemoveAll(x => x.HasAlias("addcurrency"));
                Commands.ChatCommands.RemoveAll(x => x.HasAlias("removecurrency"));
                Commands.ChatCommands.RemoveAll(x => x.HasAlias("checkcurrency"));
                Commands.ChatCommands.RemoveAll(x => x.HasAlias("givecurrency"));
            }

            base.Dispose(disposing);
        }

        #region Configuration

        public class Config
        {
            public int InitialCurrency { get; set; } = 100;
            public string CurrencySymbol { get; set; } = "$";

            public static string ConfigPath => Path.Combine(TShock.SavePath, "SocialFeatures.json");

            public static Config Read()
            {
                if (!File.Exists(ConfigPath))
                    return new Config();

                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }

            public static void Save(Config config)
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }

            public static void Load()
            {
                var config = Read();
                Instance.config = config;
                Save(config); // Save the config file with default values if it doesn't exist
            }
        }

        #endregion

        #region UserProfile

        public class UserProfile
        {
            public string PlayerName { get; set; }

            public UserProfile(string playerName)
            {
                PlayerName = playerName;
            }
        }

        #endregion
    }
}
