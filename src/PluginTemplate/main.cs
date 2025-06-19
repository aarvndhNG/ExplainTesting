using System;
using System.Collections.Generic;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace RandomRewardPlugin
{
    [ApiVersion(2, 1)]
    public class RandomRewardPlugin : TerrariaPlugin
    {
        public override string Name => "RandomRewardPlugin";
        public override string Author => "YourName";
        public override string Description => "A TShock plugin that allows owners to give a random item reward to a player.";
        public override Version Version => new Version(1, 0, 0, 0);

        private readonly Random _random = new Random();
        private readonly List<(int itemId, int minStack, int maxStack)> _rewardItems = new List<(int, int, int)>
        {
            (19, 1, 10),   // Iron Bar
            (20, 1, 10),   // Copper Bar
            (21, 1, 10),   // Silver Bar
            (22, 1, 10),   // Gold Bar
            (117, 10, 50), // Heart Crystal
            (29, 5, 20),   // Life Crystal
            (75, 10, 30)   // Mana Star
        };

        public RandomRewardPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("randomreward.give", RewardCommand, "reward")
            {
                HelpText = "Gives a random item reward to a player. Usage: /reward <player>",
                Permissions = new List<string> { "randomreward.give" }
            });
        }

        private void RewardCommand(CommandArgs args)
        {
            if (!args.Player.HasPermission("randomreward.give"))
            {
                args.Player.SendErrorMessage("You do not have permission to use this command.");
                return;
            }

            if (args.Parameters.Count != 1)
            {
                args.Player.SendErrorMessage("Usage: /reward <player>");
                return;
            }

            string targetName = args.Parameters[0];
            TSPlayer target = TShock.Players.FindPlayer(targetName);

            if (target == null)
            {
                args.Player.SendErrorMessage("Player not found or not online.");
                return;
            }

            if (!target.IsLoggedIn)
            {
                args.Player.SendErrorMessage("Target player must be logged in.");
                return;
            }

            // Select a random item from the reward pool
            var reward = _rewardItems[_random.Next(_rewardItems.Count)];
            int stackSize = _random.Next(reward.minStack, reward.maxStack + 1);

            // Give the item to the player
            target.GiveItem(reward.itemId, stackSize);

            // Notify both players
            target.SendSuccessMessage($"You received {stackSize} x {Item.GetItemName(reward.itemId)} as a reward!");
            args.Player.SendSuccessMessage($"You gave {target.Name} {stackSize} x {Item.GetItemName(reward.itemId)}.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // No hooks to deregister
            }
            base.Dispose(disposing);
        }
    }
}
