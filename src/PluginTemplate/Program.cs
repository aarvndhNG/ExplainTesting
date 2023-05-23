using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace PlayerShopsPlugin
{
    [ApiVersion(2, 1)]
    public class PlayerShopsPlugin : TerrariaPlugin
    {
        public override string Name => "PlayerShopsPlugin";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "YourName";
        public override string Description => "Enable players to set up their own shops where they can sell items to other players in a centralized marketplace.";

        private Dictionary<string, Shop> playerShops;

        public PlayerShopsPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            playerShops = new Dictionary<string, Shop>();

            TShockAPI.Commands.ChatCommands.Add(new Command("playershops.define", DefineShop, "define"));
            TShockAPI.Commands.ChatCommands.Add(new Command("playershops.create", CreateShop, "create"));
            TShockAPI.Commands.ChatCommands.Add(new Command("playershops.remove", RemoveShop, "remove"));
            TShockAPI.Commands.ChatCommands.Add(new Command("playershops.additem", AddItem, "additem"));
            TShockAPI.Commands.ChatCommands.Add(new Command("playershops.removeitem", RemoveItem, "removeitem"));
            TShockAPI.Commands.ChatCommands.Add(new Command("playershops.browse", BrowseShops, "browse"));
            TShockAPI.Commands.ChatCommands.Add(new Command("playershops.buy", BuyItem, "buy"));

            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
        }

        public override void DeInitialize()
        {
            playerShops.Clear();
        }

        private void OnGetData(GetDataEventArgs args)
        {
    var player = TShock.Players[args.Msg.whoAmI];

    if (player == null || !player.Active)
        return;

    var session = player.GetPlayerSession();

    if (args.MsgID == PacketTypes.Tile)
    {
        if (session.DefiningShop)
        {
            var packet = new TileEditPacket(args.Msg.readBuffer, args.Index, args.Length);

            if (packet.Action == TileEditAction.Place)
            {
                var tileX = packet.X + Main.tileFrameImportant[packet.Type] * 16;
                var tileY = packet.Y + Main.tileFrameImportant[packet.Type] * 16;

                if (session.ShopArea[0] == Point.Zero)
                {
                    session.SetShopCorner(1, tileX, tileY);
                    player.SendSuccessMessage("First shop corner set at X: {0}, Y: {1}. Place the second corner.", tileX, tileY);
                }
                else if (session.ShopArea[1] == Point.Zero)
                {
                    session.SetShopCorner(2, tileX, tileY);
                    player.SendSuccessMessage("Second shop corner set at X: {0}, Y: {1}. Shop area defined.", tileX, tileY);
                    session.DefiningShop = false;
                }
            }
        }
    }
}


        private void DefineShop(CommandArgs args)
        {
            var player = args.Player;
            var session = player.GetPlayerSession();

            // Check if player is already defining a shop
            if (session.DefiningShop)
            {
                player.SendErrorMessage("You are already defining a shop.");
                return;
            }

            // Set the defining shop flag
            session.DefiningShop = true;

            // Set the first and second corner of the shop area
            session.SetShopCorner(1, player.TileX, player.TileY);

            player.SendSuccessMessage("Select the second corner of your shop by placing a block.");
        }

        private void CreateShop(CommandArgs args)
        {
            var player = args.Player;
            var shopName = args.Parameters.GetParameter(0);

            // Check if player is defining a shop
            var session = player.GetPlayerSession();
            if (!session.DefiningShop)
            {
                player.SendErrorMessage("You need to define a shop first.");
                return;
            }

            // Check if shop name is provided
            if (string.IsNullOrWhiteSpace(shopName))
            {
                player.SendErrorMessage("You must provide a name for your shop.");
                return;
            }

            // Check if shop name already exists
            if (playerShops.ContainsKey(shopName))
            {
                player.SendErrorMessage("A shop with that name already exists.");
                return;
            }

            // Get the shop area coordinates
            var x1 = session.ShopArea[0].X;
            var y1 = session.ShopArea[0].Y;
            var x2 = session.ShopArea[1].X;
            var y2 = session.ShopArea[1].Y;

            // Create the shop
            var shop = new Shop(shopName, x1, y1, x2, y2);

            // Add the shop to the dictionary
            playerShops.Add(shopName, shop);

            // Reset the player's shop session
            session.ResetShopSession();

            player.SendSuccessMessage($"Shop '{shopName}' created successfully.");
        }

        private void RemoveShop(CommandArgs args)
        {
            var player = args.Player;
            var shopName = args.Parameters.GetParameter(0);

            // Check if shop name is provided
            if (string.IsNullOrWhiteSpace(shopName))
            {
                player.SendErrorMessage("You must provide the name of the shop to remove.");
                return;
            }

            // Check if shop exists
            if (!playerShops.ContainsKey(shopName))
            {
                player.SendErrorMessage("Shop not found.");
                return;
            }

            // Remove the shop
            playerShops.Remove(shopName);

            player.SendSuccessMessage($"Shop '{shopName}' removed successfully.");
        }

        private void AddItem(CommandArgs args)
        {
            var player = args.Player;
            var shopName = args.Parameters.GetParameter(0);
            var itemName = args.Parameters.GetParameter(1);
            var price = args.Parameters.GetParameter(2);

            // Check if shop name is provided
            if (string.IsNullOrWhiteSpace(shopName))
            {
                player.SendErrorMessage("You must provide the name of the shop to add an item.");
                return;
            }

            // Check if item name is provided
            if (string.IsNullOrWhiteSpace(itemName))
            {
                player.SendErrorMessage("You must provide the name of the item to add.");
                return;
            }

            // Check if price is provided
            if (string.IsNullOrWhiteSpace(price) || !int.TryParse(price, out var itemPrice))
            {
                player.SendErrorMessage("You must provide a valid price for the item.");
                return;
            }

            // Check if shop exists
            if (!playerShops.ContainsKey(shopName))
            {
                player.SendErrorMessage("Shop not found.");
                return;
            }

            // Add the item to the shop
            var shop = playerShops[shopName];
            shop.AddItem(itemName, itemPrice);

            player.SendSuccessMessage($"Item '{itemName}' added to shop '{shopName}' successfully.");
        }

        private void RemoveItem(CommandArgs args)
        {
            var player = args.Player;
            var shopName = args.Parameters.GetParameter(0);
            var itemName = args.Parameters.GetParameter(1);

            // Check if shop name is provided
            if (string.IsNullOrWhiteSpace(shopName))
            {
                player.SendErrorMessage("You must provide the name of the shop to remove an item.");
                return;
            }

            // Check if item name is provided
            if (string.IsNullOrWhiteSpace(itemName))
            {
                player.SendErrorMessage("You must provide the name of the item to remove.");
                return;
            }

            // Check if shop exists
            if (!playerShops.ContainsKey(shopName))
            {
                player.SendErrorMessage("Shop not found.");
                return;
            }

            // Remove the item from the shop
            var shop = playerShops[shopName];
            shop.RemoveItem(itemName);

            player.SendSuccessMessage($"Item '{itemName}' removed from shop '{shopName}' successfully.");
        }

        private void BrowseShops(CommandArgs args)
        {
            var player = args.Player;

            if (playerShops.Count == 0)
            {
                player.SendInfoMessage("No shops available.");
                return;
            }

            player.SendInfoMessage("Available Shops:");
            foreach (var shop in playerShops.Values)
            {
                player.SendInfoMessage($"- {shop.Name} (Owner: {shop.Owner})");
            }
        }

        private void BuyItem(CommandArgs args)
        {
            var player = args.Player;
            var shopName = args.Parameters.GetParameter(0);
            var itemName = args.Parameters.GetParameter(1);
            var amount = args.Parameters.GetInt(2);

            // Check if shop name is provided
            if (string.IsNullOrWhiteSpace(shopName))
            {
                player.SendErrorMessage("You must provide the name of the shop to buy from.");
                return;
            }

            // Check if item name is provided
            if (string.IsNullOrWhiteSpace(itemName))
            {
                player.SendErrorMessage("You must provide the name of the item to buy.");
                return;
            }

            // Check if amount is provided and valid
            if (amount <= 0)
            {
                player.SendErrorMessage("You must specify a valid amount to buy.");
                return;
            }

            // Check if shop exists
            if (!playerShops.ContainsKey(shopName))
            {
                player.SendErrorMessage("Shop not found.");
                return;
            }

            // Check if player has enough money
            var shop = playerShops[shopName];
            var item = shop.GetItem(itemName);
            if (item == null)
            {
                player.SendErrorMessage("Item not found in the shop.");
                return;
            }

            var totalPrice = item.Price * amount;
            if (player.TPlayer.Money < totalPrice)
            {
                player.SendErrorMessage("You don't have enough money to buy this item.");
                return;
            }

            // Make the purchase
            player.TPlayer.Money -= totalPrice;
            player.GiveItem(item.ItemId, item.Name, amount);

            player.SendSuccessMessage($"You bought {amount} {item.Name}(s) from shop '{shopName}' for {totalPrice} currency.");
        }
    }

    public class Shop
    {
        public string Name { get; }
        public string Owner { get; }
        public Rectangle ShopArea { get; }

        private Dictionary<string, ShopItem> shopItems;

        public Shop(string name, int x1, int y1, int x2, int y2)
        {
            Name = name;
            Owner = TShockAPI.TShock.Players[TShockAPI.TShock.Players.FindIndex(p => p.Name == TShockAPI.TShock.Players[0].Name)].Name;
            ShopArea = new Rectangle(x1, y1, Math.Abs(x2 - x1), Math.Abs(y2 - y1));

            shopItems = new Dictionary<string, ShopItem>();
        }

        public void AddItem(string itemName, int price)
        {
            if (shopItems.ContainsKey(itemName))
                shopItems[itemName].Price = price;
            else
                shopItems.Add(itemName, new ShopItem(itemName, price));
        }

        public void RemoveItem(string itemName)
        {
            shopItems.Remove(itemName);
        }

        public ShopItem GetItem(string itemName)
        {
            return shopItems.TryGetValue(itemName, out var item) ? item : null;
        }
    }

    public class ShopItem
    {
        public string Name { get; }
        public int Price { get; set; }

        public ShopItem(string name, int price)
        {
            Name = name;
            Price = price;
        }
    }

    public class PlayerSession
    {
        public bool DefiningShop { get; set; }
        public Point[] ShopArea { get; set; }

        public PlayerSession()
        {
            ResetShopSession();
        }

        public void ResetShopSession()
        {
            DefiningShop = false;
            ShopArea = new Point[2];
        }

        public void SetShopCorner(int corner, int x, int y)
        {
            if (corner >= 1 && corner <= 2)
                ShopArea[corner - 1] = new Point(x, y);
        }
    }

    public static class PlayerExtensions
    {
        private const string ShopSessionKey = "PlayerSession";

        public static PlayerSession GetPlayerSession(this TSPlayer player)
        {
            var session = player.GetData<PlayerSession>(ShopSessionKey);
            if (session == null)
            {
                session = new PlayerSession();
                player.SetData(ShopSessionKey, session);
            }
            return session;
        }
    }
}
