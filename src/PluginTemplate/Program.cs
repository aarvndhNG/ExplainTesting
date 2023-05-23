using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace PlayerShopsPlugin
{
    [ApiVersion(2, 1)]
    public class PlayerShopsPlugin : TerrariaPlugin
    {
        public override string Name => "PlayerShops";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "Your Name";
        public override string Description => "Enable players to set up their own shops where they can sell items to other players in a centralized marketplace.";

        private Dictionary<int, Shop> playerShops = new Dictionary<int, Shop>();

        public PlayerShopsPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("playershops.create", CreateShop, "shopcreate"));
            Commands.ChatCommands.Add(new Command("playershops.remove", RemoveShop, "shopremove"));
            Commands.ChatCommands.Add(new Command("playershops.additem", AddItem, "shopadditem"));
            Commands.ChatCommands.Add(new Command("playershops.removeitem", RemoveItem, "shopremoveitem"));
            Commands.ChatCommands.Add(new Command("playershops.browse", BrowseShops, "shopbrowse"));
            Commands.ChatCommands.Add(new Command("playershops.buy", BuyItem, "shopbuy"));
            Commands.ChatCommands.Add(new Command("playershops.receive", ReceiveMoney, "shopreceive"));
            Commands.ChatCommands.Add(new Command("playershops.leaderboard", ShowLeaderboard, "shopleaderboard"));
            Commands.ChatCommands.Add(new Command("playershops.shopinfo", ShopInfo, "shopinfo"));
            Commands.ChatCommands.Add(new Command("playershops.check", CheckBalance, "check"));
        }

        public override void DeInitialize()
        {
           playerShops.Clear();
           base.DeInitialize();
         }


        private void CreateShop(CommandArgs args)
        {
            int playerId = args.Player.Index;
            string shopName = args.Parameters.Count > 0 ? args.Parameters[0] : string.Empty;

            if (playerShops.ContainsKey(playerId))
            {
                args.Player.SendErrorMessage("You already have a shop.");
                return;
            }

            if (string.IsNullOrEmpty(shopName))
            {
                args.Player.SendErrorMessage("Please provide a shop name.");
                return;
            }

            Shop shop = new Shop(shopName, playerId);
            playerShops.Add(playerId, shop);
            args.Player.SendSuccessMessage($"Shop '{shopName}' created successfully with ID: {shop.Id}");
            SaveConfig();
        }

        private void RemoveShop(CommandArgs args)
        {
            int playerId = args.Player.Index;

            if (!playerShops.ContainsKey(playerId))
            {
                args.Player.SendErrorMessage("You don't have a shop.");
                return;
            }

            Shop shop = playerShops[playerId];
            playerShops.Remove(playerId);
            args.Player.SendSuccessMessage($"Shop '{shop.Name}' (ID: {shop.Id}) removed successfully.");
            SaveConfig();
        }

        private void AddItem(CommandArgs args)
        {
            int playerId = args.Player.Index;

            if (!playerShops.ContainsKey(playerId))
            {
                args.Player.SendErrorMessage("You don't have a shop.");
                return;
            }

            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /shopadditem [item name] [price]");
                return;
            }

            string itemName = args.Parameters[0];
            int price;

            if (!int.TryParse(args.Parameters[1], out price))
            {
                args.Player.SendErrorMessage("Invalid price specified.");
                return;
            }

            Shop shop = playerShops[playerId];
            Item item = TShock.Utils.GetItemByIdOrName(itemName);

            if (item.netID <= 0)
            {
                args.Player.SendErrorMessage("Invalid item specified.");
                return;
            }

            shop.Items.Add(new ShopItem(item.netID, item.Name, price));
            args.Player.SendSuccessMessage($"Item '{item.Name}' added to your shop.");
            SaveConfig();
        }

        private void RemoveItem(CommandArgs args)
        {
            int playerId = args.Player.Index;

            if (!playerShops.ContainsKey(playerId))
            {
                args.Player.SendErrorMessage("You don't have a shop.");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /shopremoveitem [item name]");
                return;
            }

            string itemName = args.Parameters[0];
            Shop shop = playerShops[playerId];
            Item item = TShock.Utils.GetItemByIdOrName(itemName);

            if (item.netID <= 0)
            {
                args.Player.SendErrorMessage("Invalid item specified.");
                return;
            }

            bool removed = shop.Items.RemoveAll(i => i.NetId == item.netID) > 0;

            if (removed)
            {
                args.Player.SendSuccessMessage($"Item '{item.Name}' removed from your shop.");
                SaveConfig();
            }
            else
            {
                args.Player.SendErrorMessage($"Item '{item.Name}' was not found in your shop.");
            }
        }

        private void BrowseShops(CommandArgs args)
        {
            if (playerShops.Count == 0)
            {
                args.Player.SendInfoMessage("No shops are available.");
                return;
            }

            args.Player.SendInfoMessage("Shops available:");
            foreach (var shop in playerShops.Values)
            {
                args.Player.SendInfoMessage($"ID: {shop.Id} | Name: {shop.Name} | Owner: {TShock.Players[shop.OwnerId].Name}");
            }
        }

        private void BuyItem(CommandArgs args)
        {
            int playerId = args.Player.Index;

            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /shopbuy [shop id] [item name]");
                return;
            }

            int shopId;
            if (!int.TryParse(args.Parameters[0], out shopId))
            {
                args.Player.SendErrorMessage("Invalid shop ID specified.");
                return;
            }

            string itemName = args.Parameters[1];
            Shop shop;
            if (!playerShops.TryGetValue(shopId, out shop))
            {
                args.Player.SendErrorMessage("Shop not found.");
                return;
            }

            Item item = shop.Items.Find(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                args.Player.SendErrorMessage("Item not found in the shop.");
                return;
            }

            if (args.Player.BankAccount.Balance < item.Price)
            {
                args.Player.SendErrorMessage("Insufficient funds.");
                return;
            }

            TShock.Players[shop.OwnerId].BankAccount.Deposit(item.Price);
            args.Player.BankAccount.Withdraw(item.Price);
            args.Player.GiveItem(item.NetId, item.Name, item.Stack);
            args.Player.SendSuccessMessage($"You bought {item.Name} from {TShock.Players[shop.OwnerId].Name}'s shop.");
        }

        private void ReceiveMoney(CommandArgs args)
        {
            int playerId = args.Player.Index;

            if (!playerShops.ContainsKey(playerId))
            {
                args.Player.SendErrorMessage("You don't have a shop.");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /shopreceivemoney [amount]");
                return;
            }

            int amount;
            if (!int.TryParse(args.Parameters[0], out amount) || amount < 0)
            {
                args.Player.SendErrorMessage("Invalid amount specified.");
                return;
            }

            if (args.Player.BankAccount.Balance < amount)
            {
                args.Player.SendErrorMessage("Insufficient funds.");
                return;
            }

            Shop shop = playerShops[playerId];
            TShock.Players[shop.OwnerId].BankAccount.Deposit(amount);
            args.Player.BankAccount.Withdraw(amount);
            args.Player.SendSuccessMessage($"You received {amount} coins.");
        }

        private void SaveConfig()
        {
            var config = new Config
            {
                PlayerShops = playerShops,
                ShopItems = shopItems
            };
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }

        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
                return;

            string json = File.ReadAllText(ConfigPath);
            var config = JsonConvert.DeserializeObject<Config>(json);

            if (config != null)
            {
                playerShops = config.PlayerShops;
                shopItems = config.ShopItems;
            }
        }

        private class Shop
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int OwnerId { get; set; }
            public List<ShopItem> Items { get; set; }

            public Shop(int id, string name, int ownerId)
            {
                Id = id;
                Name = name;
                OwnerId = ownerId;
                Items = new List<ShopItem>();
            }
        }

        private class ShopItem
        {
            public int NetId { get; set; }
            public string Name { get; set; }
            public int Price { get; set; }

            public ShopItem(int netId, string name, int price)
            {
                NetId = netId;
                Name = name;
                Price = price;
            }
        }

        private class Config
        {
            public Dictionary<int, Shop> PlayerShops { get; set; }
            public List<ShopItem> ShopItems { get; set; }
        }
    }
}
