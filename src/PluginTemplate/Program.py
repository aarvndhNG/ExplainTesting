import clr
clr.AddReference("TerrariaServerAPI")
from TerrariaServerAPI import *

class ShoppingSystem:
    def __init__(self):
        TShockAPI.Hooks.GameHooks.Join += self.on_join

    def on_join(self, args):
        player = TShock.Players[args.Who]

        # Check if player needs to be given starting currency
        if player.Name not in Config.keys():
            Config[player.Name] = 100
            TShock.Utils.Broadcast(f"{player.Name} has received 100 starting currency.", Color.Lime)

    def cmd_shop(self, args, player):
        if len(args) < 2:
            player.SendErrorMessage("Invalid syntax! Proper syntax: /shop <item name>")
            return

        item_name = " ".join(args[1:]).lower()
        item_price = Config["Shop"].get(item_name)

        if not item_price:
            player.SendErrorMessage("Item not found in shop!")
            return

        if Config[player.Name] < item_price:
            player.SendErrorMessage("You do not have enough currency to purchase this item!")
            return

        item_id = TShock.Utils.GetItemByName(item_name)[0].type
        player.GiveItem(item_id)
        Config[player.Name] -= item_price
        TShock.Utils.Broadcast(f"{player.Name} has purchased {item_name} for {item_price} currency.", Color.Lime)

    def cmd_currency(self, args, player):
        player.SendMessage(f"You currently have {Config[player.Name]} currency.", Color.Yellow)

ShoppingSystem()
