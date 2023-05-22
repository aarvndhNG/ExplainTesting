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
        private List<string> globalChatChannels;

        public override string Name => "SocialFeaturesPlugin";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "Your Name";
        public override string Description => "Adds social features to your TShock server.";

        public SocialFeaturesPlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            playerProfiles = new Dictionary<string, UserProfile>();
            friendLists = new Dictionary<string, List<string>>();
            playerScores = new Dictionary<string, int>();
            globalChatChannels = new List<string>();

            // Register chat commands
            Commands.ChatCommands.Add(new Command("profile", ProfileCommand, "profile"));
            Commands.ChatCommands.Add(new Command("leaderboard", LeaderboardCommand, "leaderboard"));
            Commands.ChatCommands.Add(new Command("friend", FriendCommand, "friend"));
            Commands.ChatCommands.Add(new Command("globalchat", GlobalChatCommand, "globalchat"));

            // Register event handlers
            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
        }

        private void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            string playerName = args.Player.Name;

            // Initialize player profile
            if (!playerProfiles.ContainsKey(playerName))
                playerProfiles.Add(playerName, new UserProfile(playerName));

            // Initialize friend list
            if (!friendLists.ContainsKey(playerName))
                friendLists.Add(playerName, new List<string>());

            // Initialize player score
            if (!playerScores.ContainsKey(playerName))
                playerScores.Add(playerName, 0);
        }

        private void OnNetGreetPlayer(GreetPlayerEventArgs args)
        {
            TSPlayer player = TShock.Players[args.Who];

            // Send welcome message and instructions to the player
            player.SendInfoMessage("Welcome to the server!");
            player.SendInfoMessage("Use /profile to view your profile.");
            player.SendInfoMessage("Use /leaderboard to view the server leaderboard.");
            player.SendInfoMessage("Use /friend to manage your friends list.");
            player.SendInfoMessage("Use /globalchat to switch to global chat channel.");
        }

        private void ProfileCommand(CommandArgs args)
        {
            string playerName = args.Player.Name;

            // Retrieve and display player profile information
            UserProfile profile = playerProfiles[playerName];
            args.Player.SendInfoMessage($"Profile Information - Name: {profile.Name}, Level: {profile.Level}, Exp: {profile.Exp}");
        }

        private void LeaderboardCommand(CommandArgs args)
        {
            // Retrieve and display the server leaderboard
            List<string> leaderboard = GetLeaderboard();
            args.Player.SendInfoMessage("Server Leaderboard:");
            for (int i = 0; i < leaderboard.Count; i++)
            {
                string entry = $"{i + 1}. {leaderboard[i]}";
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
                    args.Player.SendInfoMessage("Your Friends:");
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

            // Switch to global chat channel
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

        private List<string> GetLeaderboard()
        {
            // Retrieve and sort player scores
            List<string> leaderboard = new List<string>();
            foreach (KeyValuePair<string, int> entry in playerScores)
                leaderboard.Add($"{entry.Key}: {entry.Value}");

            leaderboard.Sort((a, b) =>
            {
                string[] partsA = a.Split(':');
                string[] partsB = b.Split(':');
                int scoreA = int.Parse(partsA[1].Trim());
                int scoreB = int.Parse(partsB[1].Trim());
                return scoreB.CompareTo(scoreA);
            });

            return leaderboard;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of resources, clean up connections, etc.
                playerProfiles.Clear();
                friendLists.Clear();
                playerScores.Clear();
                globalChatChannels.Clear();

                PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnNetGreetPlayer);
            }
            base.Dispose(disposing);
        }
    }

    public class UserProfile
    {
        public string Name { get; }
        public int Level { get; set; }
        public int Exp { get; set; }

        public UserProfile(string name)
        {
            Name = name;
            Level = 1;
            Exp = 0;
        }
    }
}
