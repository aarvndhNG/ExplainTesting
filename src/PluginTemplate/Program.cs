using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using TShockAPI.DB;

namespace inventorychecker
{
    [ApiVersion(2, 1)]
    public class inventoryviewers : TerrariaPlugin
    {
        public override string Author => "Nightklp";

        public override string Description => "Plugin to view contents of a players inventory";

        public override string Name => "InView";

        public override Version Version => new Version(1, 0, 0, 0);

        public inventoryviewers(Main game) : base(game)
        {

        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("inview.search", InventoryView, "inventoryview"));
            Commands.ChatCommands.Add(new Command("inview.search", InviewCmd, "inview", "search", "viewinv"));
            Commands.ChatCommands.Add(new Command("inview.edit", EditInventory, "editinv"));
        }
        private void InviewCmd(CommandArgs args)
        {
            var player = args.Player;

            if (args.Parameters.Count < 1)
            {
                player.SendErrorMessage("Invalid Syntax! Proper Syntax: /inview <player> [inv|equip|misc...]");
                player.SendMessage("You can view player contents using this command\n" +
                    $"Example: /inview [c/abff96:{player.Name}] [c/96ffdc:inv] (View inventory contents)", Color.WhiteSmoke);
                return;
            }

            var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Could not find any players named \"{args.Parameters[0]}\"");
                return;
            }
            else if (players.Count > 1)
            {
                player.SendMultipleMatchError(players.Select(p => p.Name)); // Multiple Players
                return;
            }
            //else if (player == players[0])
            //{
            //    // Code to return a situation of selecting yourself
            //    return;
            //}
            else
            {

                if (args.Parameters.Count > 1)
                {

                    switch (args.Parameters[1])
                    {
                        case "inv":
                        case "inventory":
                            {
                                string hotbar = "|";
                                string inventory = "|";
                                for (int i = 0; i < NetItem.InventorySlots; i++)
                                {
                                    if (i < 10)
                                    {
                                        hotbar = hotbar + "[i/s" + players[0].TPlayer.inventory[i].stack + ":" + players[0].TPlayer.inventory[i].netID + "]|";
                                    }
                                    else
                                    if (i < NetItem.InventorySlots)
                                    {
                                        if (i == 20 || i == 30 || i == 40 || i == 50)
                                        {
                                            inventory = inventory + "[i/s" + players[0].TPlayer.inventory[i].stack + ":" + players[0].TPlayer.inventory[i].netID + "]|\n|";
                                        }
                                        inventory = inventory + "[i/s" + players[0].TPlayer.inventory[i].stack + ":" + players[0].TPlayer.inventory[i].netID + "]|";
                                    }
                                }
                                args.Player.SendMessage($"Viewing {players[0].Name}'s Inventory:\n" +
                                    $"{hotbar}\n" +
                                    $"{inventory}\n" +
                                    $"[i/s{players[0].TPlayer.trashItem.stack}:{players[0].TPlayer.trashItem.netID}]", Color.WhiteSmoke);
                            }
                            break;
                        case "equip":
                        case "equipment":
                            {
                                string list1 = "";
                                string list2 = "";
                                for (int i = 0; i < 10; i++)
                                {
                                    int ii = i + 10;
                                    if (i < 3)
                                    {
                                        list1 = list1 + "|[i/s" + players[0].TPlayer.dye[i + 3].stack + ":" + players[0].TPlayer.dye[i + 3].netID + "]|[i/s" + players[0].TPlayer.armor[ii + 3].stack + ":" + players[0].TPlayer.armor[ii + 3].netID + "]|[i/s" + players[0].TPlayer.armor[i + 3].stack + ":" + players[0].TPlayer.armor[i + 3].netID + "]|\t\t|[i/s" + players[0].TPlayer.dye[i].stack + ":" + players[0].TPlayer.dye[i].netID + "]|[i/s" + players[0].TPlayer.armor[ii].stack + ":" + players[0].TPlayer.armor[ii].netID + "]|[i/s" + players[0].TPlayer.armor[i].stack + ":" + players[0].TPlayer.armor[i].netID + "]|\n";
                                    }
                                    else
                                    {
                                        list1 = list1 + "|[i/s" + players[0].TPlayer.dye[i].stack + ":" + players[0].TPlayer.dye[i].netID + "]|[i/s" + players[0].TPlayer.armor[ii].stack + ":" + players[0].TPlayer.armor[ii].netID + "]|[i/s" + players[0].TPlayer.armor[i].stack + ":" + players[0].TPlayer.armor[i].netID + "]|\n";
                                    }
                                    if (i < 5)
                                    {
                                        list2 = list2 + "|[i/s" + players[0].TPlayer.miscDyes[i].stack + ":" + players[0].TPlayer.miscDyes[i].netID + "]|[i/s" + players[0].TPlayer.miscEquips[i].stack + ":" + players[0].TPlayer.miscEquips[i].netID + "]|\n";
                                    }
                                }
                                args.Player.SendInfoMessage($"[ {players[0].Name} ] Equipment:\n\narmor & accessory:\n{list1}\nmisc:\n{list2}", Color.Gray);
                            }
                            break;
                        default:
                            player.SendErrorMessage($"Please specify the contents to view:\n" +
                                $"Inventory, Inv\n" +
                                $"Equipment, Equip.");
                            break;
                    }
                }
                return;
            }
        }
        public void InventoryView(CommandArgs args)
        {
            TSPlayer Player = args.Player;

            if (args.Parameters.Count != 1 && args.Parameters.Count != 2)
            {
                Player.SendErrorMessage("Invalid syntax. Proper syntax: /inview [inv|equipment...] <player>");
                return;
            }
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Specify a player!");
                return;
            }
            if (args.Parameters.Count == 1)
            {
                args.Player.SendErrorMessage("Specify a type! type: <inventory,equipment>");
                return;
            }
            //argument /cmd <arg0> <arg1>
            string arg0 = args.Parameters[0];
            string arg1 = args.Parameters[1];
            //finding player
            var foundPlr = TSPlayer.FindByNameOrID(arg0);
            if (foundPlr.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            var targetplayer = foundPlr[0];

            switch (arg1)
            {
                case "help":
                    {
                        Player.SendInfoMessage("view inventory\n\n/inventoryview <Player> <type>\n\ntype: <inventory,equipment>\n\nexample:\n/inventoryview john inventory");
                        return;
                    }
                case "inventory":
                case "inv":
                    {
                        string list1 = "|";
                        string list2 = "|";
                        for (int i = 0; i < NetItem.InventorySlots; i++)
                        {
                            if (i < 10)
                            {
                                list1 = list1 + "[i/s" + targetplayer.TPlayer.inventory[i].stack + ":" + targetplayer.TPlayer.inventory[i].netID + "]|";
                            }
                            else
                            if (i < NetItem.InventorySlots)
                            {
                                if (i == 20 || i == 30 || i == 40 || i == 50)
                                {
                                    list2 = list2 + "[i/s" + targetplayer.TPlayer.inventory[i].stack + ":" + targetplayer.TPlayer.inventory[i].netID + "]|\n|";
                                }
                                list2 = list2 + "[i/s" + targetplayer.TPlayer.inventory[i].stack + ":" + targetplayer.TPlayer.inventory[i].netID + "]|";
                            }
                        }
                        Player.SendMessage($"[ {targetplayer.Name} ] inventory\n\nHotbar:\n{list1}\ninventory:\n{list2}\ntrashed:\n[i/s{Player.TPlayer.trashItem.stack}:{Player.TPlayer.trashItem.netID}]", Color.Gray);
                        return;
                    }
                case "equipment":
                case "equip":
                    {
                        string list1 = "";
                        string list2 = "";
                        for (int i = 0; i < 10; i++)
                        {
                            int ii = i + 10;
                            if (i < 3)
                            {
                                list1 = list1 + "|[i/s" + targetplayer.TPlayer.dye[i + 3].stack + ":" + targetplayer.TPlayer.dye[i + 3].netID + "]|[i/s" + targetplayer.TPlayer.armor[ii + 3].stack + ":" + targetplayer.TPlayer.armor[ii + 3].netID + "]|[i/s" + targetplayer.TPlayer.armor[i + 3].stack + ":" + targetplayer.TPlayer.armor[i + 3].netID + "]|\t\t|[i/s" + targetplayer.TPlayer.dye[i].stack + ":" + targetplayer.TPlayer.dye[i].netID + "]|[i/s" + targetplayer.TPlayer.armor[ii].stack + ":" + targetplayer.TPlayer.armor[ii].netID + "]|[i/s" + targetplayer.TPlayer.armor[i].stack + ":" + targetplayer.TPlayer.armor[i].netID + "]|\n";
                            }
                            else
                            {
                                list1 = list1 + "|[i/s" + targetplayer.TPlayer.dye[i].stack + ":" + targetplayer.TPlayer.dye[i].netID + "]|[i/s" + targetplayer.TPlayer.armor[ii].stack + ":" + targetplayer.TPlayer.armor[ii].netID + "]|[i/s" + targetplayer.TPlayer.armor[i].stack + ":" + targetplayer.TPlayer.armor[i].netID + "]|\n";
                            }
                            if (i < 5)
                            {
                                list2 = list2 + "|[i/s" + targetplayer.TPlayer.miscDyes[i].stack + ":" + targetplayer.TPlayer.miscDyes[i].netID + "]|[i/s" + targetplayer.TPlayer.miscEquips[i].stack + ":" + targetplayer.TPlayer.miscEquips[i].netID + "]|\n";
                            }
                        }
                        Player.SendMessage($"[ {targetplayer.Name} ] Equipment:\n\narmor & accessory:\n{list1}\nmisc:\n{list2}", Color.Gray);
                        return;
                    }
                default:
                    {
                        Player.SendErrorMessage("Invalid type! type: <inventory,equipment>");
                        return;
                    }

            }
            return;
        }
        private void EditInventory(CommandArgs args)
        {
            var player = args.Player;
        
            if (!player.Group.HasPermission("inview.edit"))
            {
                player.SendErrorMessage("You do not have permission to use this command.");
                return;
            }
        
            if (args.Parameters.Count < 2)
            {
                player.SendErrorMessage("Invalid syntax! Proper syntax: /editinv <player> <slot> <item id> [stack]");
                return;
            }
        
            var targetPlayers = TSPlayer.FindByNameOrID(args.Parameters[0]);
            if (targetPlayers.Count == 0)
            {
                player.SendErrorMessage($"Could not find any players named \"{args.Parameters[0]}\"");
                return;
            }
            else if (targetPlayers.Count > 1)
            {
                player.SendMultipleMatchError(targetPlayers.Select(p => p.Name)); // Multiple Players
                return;
            }
        
            if (!int.TryParse(args.Parameters[1], out int slot))
            {
                player.SendErrorMessage("Invalid slot number!");
                return;
            }
        
            if (!int.TryParse(args.Parameters[2], out int itemId))
            {
                player.SendErrorMessage("Invalid item ID!");
                return;
            }
        
            int stack = 1;
            if (args.Parameters.Count > 3 && !int.TryParse(args.Parameters[3], out stack))
            {
                player.SendErrorMessage("Invalid stack size!");
                return;
            }
        
            var item = new Item();
            item.SetDefaults(itemId);
            item.stack = stack;
        
            targetPlayers[0].TPlayer.inventory[slot] = item;
            targetPlayers[0].SendData(PacketTypes.PlayerSlot, "", targetPlayers[0].Index, slot);
            player.SendSuccessMessage($"Modified {targetPlayers[0].Name}'s inventory slot {slot} to {item.Name} ({item.netID}:{item.stack})");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deregister hooks here
            }
            base.Dispose(disposing);
        }
    }
}
