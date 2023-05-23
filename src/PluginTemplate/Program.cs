using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace PlayerShopsPlugin
{
    [ApiVersion(2, 1)]
    public class PlayerShopsPlugin : TerrariaPlugin
    {
        private Dictionary<string, Shop> playerShops;
        private Dictionary<int, int> shopTransactions;

        public override string Author => "Your Name";
        public override string Description => "Enable players to set up their own shops where they can sell items to other players in a centralized marketplace.";
        public override string Name => "PlayerShops";
        public override Version Version => new Version(1, 0, 0);

        public PlayerShopsPlugin(Main game) : base(game)
        {
            playerShops = new Dictionary<string, Shop>();
            shopTransactions = new Dictionary<int, int>();
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            GeneralHooks.ReloadEvent += OnReload;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }

        private void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("shop.create", ShopCreate, "shopcreate"));
            Commands.ChatCommands.Add(new Command("shop.remove", ShopRemove, "shopremove"));
            Commands.ChatCommands.Add(new Command("shop.additem", ShopAddItem, "shopadditem"));
            Commands.ChatCommands.Add(new Command("shop.removeitem", ShopRemoveItem, "shopremoveitem"));
            Commands.ChatCommands.Add(new Command("shop.browse", ShopBrowse, "shopbrowse"));
            Commands.ChatCommands.Add(new Command("shop.buy", ShopBuy, "shopbuy"));
            Commands.ChatCommands.Add(new Command("shop.receive", ShopReceive, "shopreceive"));
            Commands.ChatCommands.Add(new Command("shop.check", ShopCheck, "shopcheck"));
        }

        private void OnLeave(LeaveEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player == null)
                return;

            if (playerShops.ContainsKey(player.Name))
            {
                var shop = playerShops[player.Name];
                shopItems.RemoveAll(x => x.ShopId == shop.ShopId);
                SaveShops();
            }
        }

        private void OnReload(ReloadEventArgs args)
        {
            playerShops.Clear();
            shopTransactions.Clear();
            ReadConfig();
        }

        private void ShopCreate(CommandArgs args)
        {
            var playerName = args.Player.Name;

            if (playerShops.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage("You already have a shop.");
                return;
            }

            var shopName = string.Join(" ", args.Parameters);

            if (string.IsNullOrWhiteSpace(shopName))
            {
                args.Player.SendErrorMessage("Invalid shop name.");
                return;
            }

            var shop = new Shop(shopName);
            playerShops.Add(playerName, shop);

            SaveShops();

            args.Player.SendSuccessMessage($"Shop '{shopName}' created successfully. Shop ID: {shop.ShopId}");
        }

        private void ShopRemove(CommandArgs args)
        {
            var playerName = args.Player.Name;

            if (!playerShops.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage("You don't have a shop.");
                return;
            }

            playerShops.Remove(playerName);
            shopItems.RemoveAll(x => x.Owner == playerName);
            SaveShops();

            args.Player.SendSuccessMessage("Shop removed successfully.");
        }

        private void ShopAddItem(CommandArgs args)
        {
            var playerName = args.Player.Name;

            if (!playerShops.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage("You don't have a shop. Create one using /shopcreate [name].");
                return;
            }

            var shop = playerShops[playerName];
            var itemName = args.Parameters.Count > 0 ? args.Parameters[0] : "";
            var itemPrice = args.Parameters.Count > 1 && int.TryParse(args.Parameters[1], out var price) ? price : 0;

            if (string.IsNullOrWhiteSpace(itemName) || itemPrice <= 0)
            {
                args.Player.SendErrorMessage("Invalid item name or price.");
                return;
            }

            var item = TShock.Utils.GetItemByIdOrName(itemName);

            if (item == null)
            {
                args.Player.SendErrorMessage("Invalid item.");
                return;
            }

            shop.Items.Add(new ShopItem(item, itemPrice));
            SaveShops();

            args.Player.SendSuccessMessage($"Item '{item.Name}' added to your shop.");
        }

        private void ShopRemoveItem(CommandArgs args)
        {
            var playerName = args.Player.Name;

            if (!playerShops.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage("You don't have a shop. Create one using /shopcreate [name].");
                return;
            }

            var shop = playerShops[playerName];
            var itemName = string.Join(" ", args.Parameters);

            if (string.IsNullOrWhiteSpace(itemName))
            {
                args.Player.SendErrorMessage("Invalid item name.");
                return;
            }

            var item = shop.Items.FirstOrDefault(x => x.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                args.Player.SendErrorMessage("Item not found in your shop.");
                return;
            }

            shop.Items.Remove(item);
            SaveShops();

            args.Player.SendSuccessMessage($"Item '{item.Name}' removed from your shop.");
        }

        private void ShopBrowse(CommandArgs args)
        {
            var playerShopsList = playerShops.Values.ToList();

            if (playerShopsList.Count == 0)
            {
                args.Player.SendInfoMessage("There are no shops available.");
                return;
            }

            args.Player.SendInfoMessage("Available Shops:");

            foreach (var shop in playerShopsList)
            {
                args.Player.SendInfoMessage($"Shop ID: {shop.ShopId} | Owner: {shop.Owner} | Name: {shop.Name} | Created: {shop.Created}");
            }
        }

        private void ShopBuy(CommandArgs args)
        {
            var shopId = args.Parameters.Count > 1 && int.TryParse(args.Parameters[2], out var id) ? id : -1;

            if (shopId < 0)
            {
                args.Player.SendErrorMessage("Invalid shop ID.");
                return;
            }

            var shop = playerShops.Values.FirstOrDefault(x => x.ShopId == shopId);

            if (shop == null)
            {
                args.Player.SendErrorMessage("Shop not found.");
                return;
            }

            var itemName = args.Parameters.Count > 0 ? args.Parameters[0] : "";
            var amount = args.Parameters.Count > 1 && int.TryParse(args.Parameters[1], out var qty) ? qty : 1;

            if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
            {
                args.Player.SendErrorMessage("Invalid item name or amount.");
                return;
            }

            var item = shop.Items.FirstOrDefault(x => x.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                args.Player.SendErrorMessage("Item not found in the shop.");
                return;
            }

            if (item.Quantity < amount)
            {
                args.Player.SendErrorMessage("Insufficient quantity in the shop.");
                return;
            }

            var totalPrice = item.Price * amount;

            if (!EconomyPlugin.CallHook("HasBalance", args.Player.User.ID, totalPrice).Equals(true))
            {
                args.Player.SendErrorMessage("You don't have enough currency to buy this item.");
                return;
            }

            EconomyPlugin.CallHook("Subtract", args.Player.User.ID, totalPrice);

            item.Quantity -= amount;

            if (item.Quantity <= 0)
                shop.Items.Remove(item);

            var itemToGive = TShock.Utils.GetItemByIdOrName(item.Name);
            itemToGive.stack = amount;

            args.Player.GiveItem(itemToGive.netID, itemToGive.Name, itemToGive.width, itemToGive.height, itemToGive.stack, itemToGive.prefix);

            SaveShops();

            args.Player.SendSuccessMessage($"You bought {amount}x {itemToGive.Name} from shop {shop.ShopId} for {totalPrice} currency.");
        }

        private void ShopReceive(CommandArgs args)
        {
            var playerName = args.Player.Name;

            if (!playerShops.ContainsKey(playerName))
            {
                args.Player.SendErrorMessage("You don't have a shop. Create one using /shopcreate [name].");
                return;
            }

            var shop = playerShops[playerName];
            var totalSales = 0;

            foreach (var transaction in shopTransactions)
            {
                if (transaction.Key == shop.ShopId)
                {
                    totalSales += transaction.Value;
                }
            }

            if (totalSales == 0)
            {
                args.Player.SendInfoMessage("You have no sales to receive.");
                return;
            }

            EconomyPlugin.CallHook("Add", args.Player.User.ID, totalSales);

            shopTransactions.RemoveAll(x => x.Key == shop.ShopId);

            SaveShops();

            args.Player.SendSuccessMessage($"You received {totalSales} currency for your sales.");
        }

        private void ShopCheck(CommandArgs args)
        {
            var playerName = args.Player.Name;

            var balance = (int)EconomyPlugin.CallHook("GetPlayerBalance", args.Player.User.ID);
            args.Player.SendInfoMessage($"Your current balance is {balance} currency.");
        }

        private void SaveShops()
        {
            var data = new List<ShopData>();

            foreach (var shop in playerShops.Values)
            {
                var shopData = new ShopData
                {
                    Owner = shop.Owner,
                    Name = shop.Name,
                    ShopId = shop.ShopId,
                    Created = shop.Created,
                    Items = shop.Items.Select(x => new ShopItemData
                    {
                        Name = x.Name,
                        Price = x.Price,
                        Quantity = x.Quantity
                    }).ToList()
                };

                data.Add(shopData);
            }

            var json = TShockAPI.Utils.SerializeJson(data);
            File.WriteAllText(Path.Combine(TShock.SavePath, "PlayerShops.json"), json);
        }

        private void LoadShops()
        {
            var path = Path.Combine(TShock.SavePath, "PlayerShops.json");

            if (!File.Exists(path))
                return;

            var json = File.ReadAllText(path);
            var data = TShockAPI.Utils.DeserializeJson<List<ShopData>>(json);

            if (data != null)
            {
                foreach (var shopData in data)
                {
                    var shop = new Shop(shopData.Owner, shopData.Name, shopData.ShopId, shopData.Created);

                    foreach (var itemData in shopData.Items)
                    {
                        var item = new ShopItem(itemData.Name, itemData.Price, itemData.Quantity);
                        shop.Items.Add(item);
                    }

                    playerShops.Add(shopData.Owner, shop);
                }
            }
        }

        private class Shop
        {
            public string Owner { get; }
            public string Name { get; }
            public int ShopId { get; }
            public DateTime Created { get; }
            public List<ShopItem> Items { get; }

            public Shop(string owner, string name, int shopId, DateTime created)
            {
                Owner = owner;
                Name = name;
                ShopId = shopId;
                Created = created;
                Items = new List<ShopItem>();
            }

            public Shop(string owner, string name)
            {
                Owner = owner;
                Name = name;
                ShopId = GetNextShopId();
                Created = DateTime.Now;
                Items = new List<ShopItem>();
            }

            private int GetNextShopId()
            {
                int maxId = playerShops.Values.Select(x => x.ShopId).DefaultIfEmpty(0).Max();
                return maxId + 1;
            }
        }

        private class ShopItem
        {
            public string Name { get; }
            public int Price { get; }
            public int Quantity { get; set; }

            public ShopItem(string name, int price, int quantity)
            {
                Name = name;
                Price = price;
                Quantity = quantity;
            }

            public ShopItem(Item item, int price)
            {
                Name = item.Name;
                Price = price;
                Quantity = item.stack;
            }
        }

        private class ShopData
        {
            public string Owner { get; set; }
            public string Name { get; set; }
            public int ShopId { get; set; }
            public DateTime Created { get; set; }
            public List<ShopItemData> Items { get; set; }
        }

        private class ShopItemData
        {
            public string Name { get; set; }
            public int Price { get; set; }
            public int Quantity { get; set; }
        }
    }
}
