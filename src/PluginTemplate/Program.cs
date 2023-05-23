using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
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

            LoadConfig();
        }

        public override void DeInitialize()
        {
            playerShops.Clear();
            SaveConfig();
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

            Shop shop = new Shop(playerId, shopName, playerId);
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

            Shop shop = playerShops[playerId];
            int amount = shop.CalculateTotalIncome();

            if (amount <= 0)
            {
                args.Player.SendInfoMessage("No money to receive.");
                return;
            }

            args.Player.BankAccount.Deposit(amount);
            args.Player.SendSuccessMessage($"You received {amount} coins from your shop.");
            shop.ClearIncome();
            SaveConfig();
        }

        private void ShowLeaderboard(CommandArgs args)
        {
            if (playerShops.Count == 0)
            {
                args.Player.SendInfoMessage("No shops are available.");
                return;
            }

            List<Shop> sortedShops = new List<Shop>(playerShops.Values);
            sortedShops.Sort((a, b) => b.CalculateTotalIncome().CompareTo(a.CalculateTotalIncome()));

            args.Player.SendInfoMessage("Shop Leaderboard:");
            int rank = 1;
            foreach (var shop in sortedShops)
            {
                args.Player.SendInfoMessage($"{rank}. Shop ID: {shop.Id} | Owner: {TShock.Players[shop.OwnerId].Name} | Total Income: {shop.CalculateTotalIncome()} coins");
                rank++;
            }
        }

        private void ShopInfo(CommandArgs args)
        {
            int playerId = args.Player.Index;

            if (!playerShops.ContainsKey(playerId))
            {
                args.Player.SendErrorMessage("You don't have a shop.");
                return;
            }

            Shop shop = playerShops[playerId];

            args.Player.SendInfoMessage($"Shop ID: {shop.Id} | Name: {shop.Name} | Owner: {TShock.Players[shop.OwnerId].Name}");
            args.Player.SendInfoMessage("Items in the shop:");
            foreach (var item in shop.Items)
            {
                args.Player.SendInfoMessage($"Item: {item.Name} | Price: {item.Price}");
            }
        }

        private void CheckBalance(CommandArgs args)
        {
            int playerId = args.Player.Index;

            args.Player.SendInfoMessage($"Your current balance is: {args.Player.BankAccount.Balance} coins.");
        }

        private void LoadConfig()
        {
            string filePath = Path.Combine(TShock.SavePath, "playershops.json");
            if (!File.Exists(filePath))
                return;

            string json = File.ReadAllText(filePath);
            playerShops = JsonConvert.DeserializeObject<Dictionary<int, Shop>>(json);
        }

        private void SaveConfig()
        {
            string filePath = Path.Combine(TShock.SavePath, "playershops.json");
            string json = JsonConvert.SerializeObject(playerShops, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }

    public class Shop
    {
        public int Id { get; }
        public string Name { get; }
        public int OwnerId { get; }
        public List<ShopItem> Items { get; }

        public Shop(int id, string name, int ownerId)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            Items = new List<ShopItem>();
        }

        public int CalculateTotalIncome()
        {
            int total = 0;
            foreach (var item in Items)
            {
                total += item.Price * item.Stack;
            }
            return total;
        }

        public void ClearIncome()
        {
            Items.Clear();
        }
    }

    public class ShopItem
    {
        public int NetId { get; }
        public string Name { get; }
        public int Price { get; }
        public int Stack { get; }

        public ShopItem(int netId, string name, int price, int stack = 1)
        {
            NetId = netId;
            Name = name;
            Price = price;
            Stack = stack;
        }
    }
}
