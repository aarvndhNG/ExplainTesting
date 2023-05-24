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
                args.Player.SendErrorMessage("Invalid price! Price must be an integer.");
                return;
            }

            if (price <= 0)
            {
                args.Player.SendErrorMessage("Invalid price! Price must be greater than zero.");
                return;
            }

            Shop shop = playerShops[playerId];
            var item = TShock.Utils.GetItemByIdOrName(itemName);
            if (item.type == 0)
            {
                args.Player.SendErrorMessage("Invalid item name!");
                return;
            }

            shop.AddItem(new ShopItem(item.netID, price));
            args.Player.SendSuccessMessage($"Item '{item.name}' added to your shop.");
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
            var item = TShock.Utils.GetItemByIdOrName(itemName);
            if (item.type == 0)
            {
                args.Player.SendErrorMessage("Invalid item name!");
                return;
            }

            if (shop.RemoveItem(item.netID))
            {
                args.Player.SendSuccessMessage($"Item '{item.name}' removed from your shop.");
                SaveConfig();
            }
            else
            {
                args.Player.SendErrorMessage($"Item '{item.name}' not found in your shop.");
            }
        }

        private void BrowseShops(CommandArgs args)
        {
            List<Shop> shops = new List<Shop>(playerShops.Values);
            if (shops.Count == 0)
            {
                args.Player.SendInfoMessage("There are no player shops available.");
                return;
            }

            args.Player.SendInfoMessage("---- Player Shops ----");
            foreach (Shop shop in shops)
            {
                args.Player.SendInfoMessage($"ID: {shop.Id} | Owner: {TShock.Players[shop.OwnerId].Name} | Name: {shop.Name}");
            }
        }

        private void BuyItem(CommandArgs args)
        {
            int playerId = args.Player.Index;

            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /shopbuy [shop id] [item id]");
                return;
            }

            int shopId;
            if (!int.TryParse(args.Parameters[0], out shopId))
            {
                args.Player.SendErrorMessage("Invalid shop ID!");
                return;
            }

            int itemId;
            if (!int.TryParse(args.Parameters[1], out itemId))
            {
                args.Player.SendErrorMessage("Invalid item ID!");
                return;
            }

            if (!playerShops.ContainsKey(shopId))
            {
                args.Player.SendErrorMessage("Invalid shop ID!");
                return;
            }

            Shop shop = playerShops[shopId];

            if (!shop.ContainsItem(itemId))
            {
                args.Player.SendErrorMessage("Invalid item ID!");
                return;
            }

            ShopItem item = shop.GetItem(itemId);
            int price = item.Price;

            if (args.Player.BankAccount().Balance < price)
            {
                args.Player.SendErrorMessage("Insufficient funds!");
                return;
            }

            TSPlayer shopOwner = TShock.Players[shop.OwnerId];
            if (shopOwner == null)
            {
                args.Player.SendErrorMessage("Shop owner is not online.");
                return;
            }

            var purchasedItem = TShock.Utils.GetItemById(item.ItemId);

            if (purchasedItem.stack + args.Player.GiveItemCheck(item.ItemId, purchasedItem.name, purchasedItem.width, purchasedItem.height, item.Price) > purchasedItem.maxStack)
            {
                args.Player.SendErrorMessage("You don't have enough inventory space to buy this item.");
                return;
            }

            args.Player.GiveItemCheck(item.ItemId, purchasedItem.name, purchasedItem.width, purchasedItem.height, item.Price);
            args.Player.BankAccount().TransferTo(price, shopOwner.BankAccount());
            args.Player.SendSuccessMessage($"You bought {purchasedItem.name} from {shopOwner.Name} for {price} coins.");
            shopOwner.SendInfoMessage($"{args.Player.Name} bought {purchasedItem.name} for {price} coins from your shop.");

            if (shop.IsEmpty())
            {
                playerShops.Remove(shopId);
                SaveConfig();
            }
        }

        private void ReceiveMoney(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /shopreceive [amount]");
                return;
            }

            int amount;
            if (!int.TryParse(args.Parameters[0], out amount))
            {
                args.Player.SendErrorMessage("Invalid amount!");
                return;
            }

            args.Player.BankAccount().Deposit(amount);
            args.Player.SendSuccessMessage($"Received {amount} coins in your bank account.");
        }

        private void ShowLeaderboard(CommandArgs args)
        {
            List<Shop> shops = new List<Shop>(playerShops.Values);
            if (shops.Count == 0)
            {
                args.Player.SendInfoMessage("There are no player shops available.");
                return;
            }

            shops.Sort((shop1, shop2) => shop2.Sales.CompareTo(shop1.Sales));

            args.Player.SendInfoMessage("---- Player Shop Leaderboard ----");
            int rank = 1;
            foreach (Shop shop in shops)
            {
                args.Player.SendInfoMessage($"#{rank}: {TShock.Players[shop.OwnerId].Name} | Sales: {shop.Sales}");
                rank++;
            }
        }

        private void ShopInfo(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /shopinfo [shop id]");
                return;
            }

            int shopId;
            if (!int.TryParse(args.Parameters[0], out shopId))
            {
                args.Player.SendErrorMessage("Invalid shop ID!");
                return;
            }

            if (!playerShops.ContainsKey(shopId))
            {
                args.Player.SendErrorMessage("Invalid shop ID!");
                return;
            }

            Shop shop = playerShops[shopId];
            args.Player.SendInfoMessage($"Shop Info - ID: {shop.Id} | Owner: {TShock.Players[shop.OwnerId].Name} | Name: {shop.Name}");
            args.Player.SendInfoMessage("---- Shop Items ----");
            foreach (ShopItem item in shop.Items)
            {
                var shopItem = TShock.Utils.GetItemById(item.ItemId);
                args.Player.SendInfoMessage($"Item ID: {item.ItemId} | Item Name: {shopItem.name} | Price: {item.Price}");
            }
        }

        private void CheckBalance(CommandArgs args)
        {
            args.Player.SendSuccessMessage($"Your bank account balance: {args.Player.BankAccount().Balance} coins.");
        }

        private void LoadConfig()
        {
            string path = Path.Combine(TShock.SavePath, "playershops.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                playerShops = JsonConvert.DeserializeObject<Dictionary<int, Shop>>(json);
            }
        }

        private void SaveConfig()
        {
            string path = Path.Combine(TShock.SavePath, "playershops.json");
            string json = JsonConvert.SerializeObject(playerShops, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }

    public class Shop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }
        public List<ShopItem> Items { get; set; }
        public int Sales { get; set; }

        public Shop(int id, string name, int ownerId)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            Items = new List<ShopItem>();
        }

        public void AddItem(ShopItem item)
        {
            Items.Add(item);
        }

        public bool RemoveItem(int itemId)
        {
            ShopItem item = Items.Find(i => i.ItemId == itemId);
            if (item != null)
            {
                Items.Remove(item);
                return true;
            }
            return false;
        }

        public bool ContainsItem(int itemId)
        {
            return Items.Exists(i => i.ItemId == itemId);
        }

        public ShopItem GetItem(int itemId)
        {
            return Items.Find(i => i.ItemId == itemId);
        }

        public bool IsEmpty()
        {
            return Items.Count == 0;
        }
    }

    public class ShopItem
    {
        public int ItemId { get; set; }
        public int Price { get; set; }

        public ShopItem(int itemId, int price)
        {
            ItemId = itemId;
            Price = price;
        }
    }
}
