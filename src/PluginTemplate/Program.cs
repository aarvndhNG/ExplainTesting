using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace SocialFeaturesPlugin
{
    [ApiVersion(2, 1)]
    public class SocialFeaturesPlugin : TerrariaPlugin
    {
        private Dictionary<string, UserProfile> playerProfiles;
        private Dictionary<string, List<string>> friendLists;
        private Dictionary<string, int> playerScores;
        private Dictionary<string, int> playerCurrency;

        public override string Name => "SocialFeaturesPlugin";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "BlackWolf";
        public override string Description => "Adds social features to your TShock server.";

        public SocialFeaturesPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            // Initialize dictionaries
            playerProfiles = new Dictionary<string, UserProfile>();
            friendLists = new Dictionary<string, List<string>>();
            playerScores = new Dictionary<string, int>();
            playerCurrency = new Dictionary<string, int>();

            // Hook into events
            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
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
                playerCurrency[playerName] = 0;
            }
        }

        private void OnNetGreetPlayer(GreetPlayerEventArgs args)
        {
            string greeting = $"Welcome to the server, {args.Who}!";
            NetMessage.SendData((int)PacketTypes.ChatText, args.Who, -1, greeting, 255, 255, 0, 0, 0, 0, 0);
        }

        private void ProfileCommand(CommandArgs args)
        {
            string playerName = args.Player.Name;

            // Retrieve player profile and currency balance
            UserProfile profile = playerProfiles[playerName];
            int currency = playerCurrency[playerName];

            // Retrieve global chat channel if player has joined one
            string globalChatChannel = globalChatChannels.Contains(playerName) ? "Global" : "None";

            // Display player profile information and currency balance
            args.Player.SendInfoMessage($"Profile Information - Name: {profile.Name}, Level: {profile.Level}");
            args.Player.SendInfoMessage($"Currency Balance: {currency}");
            args.Player.SendInfoMessage($"Global Chat Channel: {globalChatChannel}");
        }

        private void LeaderboardCommand(CommandArgs args)
        {
            List<string> leaderboard = GetLeaderboard();

            if (leaderboard.Count == 0)
            {
                args.Player.SendInfoMessage("No scores available.");
            }
            else
            {
                args.Player.SendInfoMessage("Leaderboard:");
                foreach (string entry in leaderboard)
                    args.Player.SendInfoMessage(entry);
            }
        }

        private void FriendCommand(CommandArgs args)
        {
            string playerName = args.Player.Name;
            string friendName = args.Parameters.Count > 0 ? args.Parameters[0] : "";

            if (string.IsNullOrWhiteSpace(friendName))
            {
                // Display friend list
                List<string> friends = friendLists[playerName];
                if (friends.Count == 0)
                {
                    args.Player.SendInfoMessage("You have no friends.");
                }
                else
                {
                    args.Player.SendInfoMessage("Friend List:");
                    foreach (string friend in friends)
                        args.Player.SendInfoMessage(friend);
                }
            }
            else
            {
                // Add or remove friend
                if (friendLists[playerName].Contains(friendName))
                {
                    friendLists[playerName].Remove(friendName);
                    args.Player.SendSuccessMessage($"Removed {friendName} from your friends list.");
                }
                else
                {
                    friendLists[playerName].Add(friendName);
                    args.Player.SendSuccessMessage($"Added {friendName} to your friends list.");
                }
            }
        }

        private void GlobalChatCommand(CommandArgs args)
        {
            string playerName = args.Player.Name;

            if (globalChatChannels.Contains(playerName))
            {
                globalChatChannels.Remove(playerName);
                args.Player.SendSuccessMessage("You have left the global chat channel.");
            }
            else
            {
                globalChatChannels.Add(playerName);
                args.Player.SendSuccessMessage("You have joined the global chat channel.");
            }
        }

        private void AddEcoCommand(CommandArgs args)
        {
            if (args.Player.Group.HasPermission("admin"))
            {
                string playerName = args.Parameters.Count > 0 ? args.Parameters[0] : "";
                int amount = args.Parameters.Count > 1 ? int.Parse(args.Parameters[1]) : 0;

                if (string.IsNullOrWhiteSpace(playerName) || amount <= 0)
                {
                    args.Player.SendErrorMessage("Invalid command usage. Proper usage: /addeco <player name> <amount>");
                    return;
                }

                if (playerCurrency.ContainsKey(playerName))
                {
                    playerCurrency[playerName] += amount;
                    args.Player.SendSuccessMessage($"Added {amount} to {playerName}'s currency balance.");
                }
                else
                {
                    args.Player.SendErrorMessage($"Player {playerName} not found.");
                }
            }
            else
            {
                args.Player.SendErrorMessage("You do not have permission to use this command.");
            }
        }

        private void RemoveEcoCommand(CommandArgs args)
        {
            if (args.Player.Group.HasPermission("admin"))
            {
                string playerName = args.Parameters.Count > 0 ? args.Parameters[0] : "";
                int amount = args.Parameters.Count > 1 ? int.Parse(args.Parameters[1]) : 0;

                if (string.IsNullOrWhiteSpace(playerName) || amount <= 0)
                {
                    args.Player.SendErrorMessage("Invalid command usage. Proper usage: /removeeco <player name> <amount>");
                    return;
                }

                if (playerCurrency.ContainsKey(playerName))
                {
                    if (playerCurrency[playerName] >= amount)
                    {
                        playerCurrency[playerName] -= amount;
                        args.Player.SendSuccessMessage($"Removed {amount} from {playerName}'s currency balance.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage($"Player {playerName} does not have enough currency.");
                    }
                }
                else
                {
                    args.Player.SendErrorMessage($"Player {playerName} not found.");
                }
            }
            else
            {
                args.Player.SendErrorMessage("You do not have permission to use this command.");
            }
        }

        private void CheckEcoCommand(CommandArgs args)
        {
            string playerName = args.Player.Name;
            int currency = playerCurrency[playerName];

            args.Player.SendInfoMessage($"Your currency balance: {currency}");
        }

        private void GiveEcoCommand(CommandArgs args)
        {
            string playerName = args.Player.Name;
            string targetPlayerName = args.Parameters.Count > 0 ? args.Parameters[0] : "";
            int amount = args.Parameters.Count > 1 ? int.Parse(args.Parameters[1]) : 0;

            if (string.IsNullOrWhiteSpace(targetPlayerName) || amount <= 0)
            {
                args.Player.SendErrorMessage("Invalid command usage. Proper usage: /giveeco <player name> <amount>");
                return;
            }

            if (playerCurrency.ContainsKey(targetPlayerName))
            {
                if (playerCurrency[playerName] >= amount)
                {
                    playerCurrency[playerName] -= amount;
                    playerCurrency[targetPlayerName] += amount;
                    args.Player.SendSuccessMessage($"Transferred {amount} from your balance to {targetPlayerName}.");
                }
                else
                {
                    args.Player.SendErrorMessage("You do not have enough currency.");
                }
            }
            else
            {
                args.Player.SendErrorMessage($"Player {targetPlayerName} not found.");
            }
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnNetGreetPlayer);
            }
            base.Dispose(disposing);
        }
    }

    public class UserProfile
    {
        public string Name { get; set; }
        public int Level { get; set; }

        public UserProfile(string name)
        {
            Name = name;
            Level = 1;
        }
    }
}
