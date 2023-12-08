using System;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TShockAPI;

namespace MyRewardPlugin
{
    [ApiVersion(2, 1)]
    public class MyRewardPlugin : TerrariaPlugin
    {
        private Dictionary<int, (string name, int price)> shopItems = new Dictionary<int, (string name, int price)>();

        public MyRewardPlugin(Main game) : base(game)
        {
            Order = 10;
        }

        public override void Initialize()
        {
            // Register commands
            Commands.ChatCommands.Add(new Command("reward", RewardCommand, "reward", "points"));
            Commands.ChatCommands.Add(new Command("redeem", RedeemCommand, "redeem", "points"));
            Commands.ChatCommands.Add(new Command("shop", ShopCommand, "shop", "items"));

            // Register hooks
            NetHooks.PlayerConnect += OnPlayerConnect;
            PlayerHooks.PlayerKilled += OnPlayerKillMob;

            // Load shop items from configuration file
            LoadShopItems();
        }

        private void LoadShopItems()
        {
            string configPath = Path.Combine(TShock.SavePath, "MyRewardPlugin.json");
            if (!File.Exists(configPath))
            {
                Log.Info("MyRewardPlugin configuration file not found. Using default shop items.");
                return;
            }

            try
            {
                shopItems = JsonConvert.DeserializeObject<Dictionary<int, (string name, int price)>>(File.ReadAllText(configPath));
            }
            catch (Exception ex)
            {
                Log.Error("Error loading MyRewardPlugin configuration: " + ex.Message);
            }
        }

        private void RewardCommand(CommandArgs args)
        {
            // Check if player has enough points
            if (TShock.Players[args.Player].GetDouble("points") < 10)
            {
                args.Player.SendErrorMessage("You don't have enough points to redeem that reward!");
                return;
            }

            // Give player the reward
            args.Player.GiveItem(12, 1); // Example reward: 1 Gold Coin
            args.Player.SendSuccessMessage("You have redeemed your points for the reward!");

            // Subtract points from player
            TShock.Players[args.Player].SetDouble("points", TShock.Players[args.Player].GetDouble("points") - 10);
        }

        private void RedeemCommand(CommandArgs args)
{
    // Parse item ID from argument
    int itemId;
    if (!int.TryParse(args.Parameters[0], out itemId))
    {
        args.Player.SendErrorMessage("Invalid item ID specified!");
        return;
    }

    // Check if item exists in shop
    if (!shopItems.ContainsKey(itemId))
    {
        args.Player.SendErrorMessage("Item with that ID is not available for redemption!");
        return;
    }

    // Check if player has enough points
    if (TShock.Players[args.Player].GetDouble("points") < shopItems[itemId].price)
    {
        args.Player.SendErrorMessage("You don't have enough points to redeem this item!");
        return;
    }

    // Give player the item
    args.Player.GiveItem(itemId, 1);
    args.Player.SendSuccessMessage("You have redeemed your points for the " + shopItems[itemId].name + "!");

    // Subtract points from player
    TShock.Players[args.Player].SetDouble("points", TShock.Players[args.Player].GetDouble("points") - shopItems[itemId].price);
}

        private void ShopCommand(CommandArgs args)
        {
            if (shopItems.Count == 0)
            {
                args.Player.SendErrorMessage("There are no items available in the shop!");
                return;
            }

            args.Player.SendInfoMessage("Available shop items:");
            foreach (var item in shopItems)
            {
                args.Player.SendInfoMessage($" - {item.Value.name} (ID: {item.Key}) - Cost: {item.Value.price} points");
            }
        }

        private void OnPlayerConnect(NetInfo netInfo, LegacyNetPlayer player)
        {
            // Check if player has points stored
            if (!TShock.Players[player].HasData("points"))
            {
                TShock.Players[player].SetData("points", 0); // Set initial points to 0
            }
        }

        private void OnPlayerKillMob(object sender, PlayerKilledEventArgs e)
        {
            // Check if mob is a specific type
            if (e.KilledNPC.type == NPCID.Slime)
            {
                // Give player points for killing the mob
                TShock.Players[e.Player].SetDouble("points", TShock.Players[e.Player].GetDouble("points") + 1);
                e.Player.SendSuccessMessage("You have earned 1 point for killing a Slime!");
            }
        }
    }
}
