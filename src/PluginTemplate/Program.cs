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

        public override string Description => "Plugin to view contents of a player's inventory";

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
            Commands.ChatCommands.Add(new Command("inview.profile", ViewProfile, "profile"));
            Commands.ChatCommands.Add(new Command("inview.profile", SetProfile, "setprofile"));
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
                                    else if (i < NetItem.InventorySlots)
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
                                    $"[i/s{players[0].TPlayer.trashItem.stack}:{players[0].TPlayer.trashItem.netID}] (Trash)", Color.Aquamarine);
                                break;
                            }
                        case "equip":
                        case "equipment":
                            {
                                string armor = $"Armor: [i/s{players[0].TPlayer.armor[0].stack}:{players[0].TPlayer.armor[0].netID}] (Head) " +
                                    $"[i/s{players[0].TPlayer.armor[1].stack}:{players[0].TPlayer.armor[1].netID}] (Chest) " +
                                    $"[i/s{players[0].TPlayer.armor[2].stack}:{players[0].TPlayer.armor[2].netID}] (Legs) " +
                                    $"[i/s{players[0].TPlayer.armor[3].stack}:{players[0].TPlayer.armor[3].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.armor[4].stack}:{players[0].TPlayer.armor[4].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.armor[5].stack}:{players[0].TPlayer.armor[5].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.armor[6].stack}:{players[0].TPlayer.armor[6].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.armor[7].stack}:{players[0].TPlayer.armor[7].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.armor[8].stack}:{players[0].TPlayer.armor[8].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.armor[9].stack}:{players[0].TPlayer.armor[9].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.armor[10].stack}:{players[0].TPlayer.armor[10].netID}] (Accessory)\n";
                                string dye = $"Dye: [i/s{players[0].TPlayer.dye[0].stack}:{players[0].TPlayer.dye[0].netID}] (Head) " +
                                    $"[i/s{players[0].TPlayer.dye[1].stack}:{players[0].TPlayer.dye[1].netID}] (Chest) " +
                                    $"[i/s{players[0].TPlayer.dye[2].stack}:{players[0].TPlayer.dye[2].netID}] (Legs) " +
                                    $"[i/s{players[0].TPlayer.dye[3].stack}:{players[0].TPlayer.dye[3].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.dye[4].stack}:{players[0].TPlayer.dye[4].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.dye[5].stack}:{players[0].TPlayer.dye[5].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.dye[6].stack}:{players[0].TPlayer.dye[6].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.dye[7].stack}:{players[0].TPlayer.dye[7].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.dye[8].stack}:{players[0].TPlayer.dye[8].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.dye[9].stack}:{players[0].TPlayer.dye[9].netID}] (Accessory) " +
                                    $"[i/s{players[0].TPlayer.dye[10].stack}:{players[0].TPlayer.dye[10].netID}] (Accessory)\n";
                                string miscEquips = $"Misc Equip: [i/s{players[0].TPlayer.miscEquips[0].stack}:{players[0].TPlayer.miscEquips[0].netID}] " +
                                    $"[i/s{players[0].TPlayer.miscEquips[1].stack}:{players[0].TPlayer.miscEquips[1].netID}] " +
                                    $"[i/s{players[0].TPlayer.miscEquips[2].stack}:{players[0].TPlayer.miscEquips[2].netID}] " +
                                    $"[i/s{players[0].TPlayer.miscEquips[3].stack}:{players[0].TPlayer.miscEquips[3].netID}] " +
                                    $"[i/s{players[0].TPlayer.miscEquips[4].stack}:{players[0].TPlayer.miscEquips[4].netID}]\n";
                                args.Player.SendMessage($"Viewing {players[0].Name}'s Equipment:\n" +
                                    $"{armor}\n" +
                                    $"{dye}\n" +
                                    $"{miscEquips}", Color.Aquamarine);
                                break;
                            }
                        case "misc":
                        case "miscellaneous":
                            {
                                string coins = $"Coins: [i/s{players[0].TPlayer.bank.item[49].stack}:{players[0].TPlayer.bank.item[49].netID}] (Platinum) " +
                                    $"[i/s{players[0].TPlayer.bank.item[48].stack}:{players[0].TPlayer.bank.item[48].netID}] (Gold) " +
                                    $"[i/s{players[0].TPlayer.bank.item[47].stack}:{players[0].TPlayer.bank.item[47].netID}] (Silver) " +
                                    $"[i/s{players[0].TPlayer.bank.item[46].stack}:{players[0].TPlayer.bank.item[46].netID}] (Copper)\n";
                                string safe = $"Safe: [i/s{players[0].TPlayer.bank.item[40].stack}:{players[0].TPlayer.bank.item[40].netID}] " +
                                    $"[i/s{players[0].TPlayer.bank.item[41].stack}:{players[0].TPlayer.bank.item[41].netID}] " +
                                    $"[i/s{players[0].TPlayer.bank.item[42].stack}:{players[0].TPlayer.bank.item[42].netID}] " +
                                    $"[i/s{players[0].TPlayer.bank.item[43].stack}:{players[0].TPlayer.bank.item[43].netID}] " +
                                    $"[i/s{players[0].TPlayer.bank.item[44].stack}:{players[0].TPlayer.bank.item[44].netID}] " +
                                    $"[i/s{players[0].TPlayer.bank.item[45].stack}:{players[0].TPlayer.bank.item[45].netID}]\n";
                                string forge = $"Forge: [i/s{players[0].TPlayer.bank.item[39].stack}:{players[0].TPlayer.bank.item[39].netID}] " +
                                    $"[i/s{players[0].TPlayer.bank.item[38].stack}:{players[0].TPlayer.bank.item[38].netID}] " +
                                    $"[i/s{players[0].TPlayer.bank.item[37].stack}:{players[0].TPlayer.bank.item[37].netID}] " +
                                    $"[i/s{players[0].TPlayer.bank.item[36].stack}:{players[0].TPlayer.bank.item[36].netID}]\n";
                                args.Player.SendMessage($"Viewing {players[0].Name}'s Miscellaneous Items:\n" +
                                    $"{coins}\n" +
                                    $"{safe}\n" +
                                    $"{forge}", Color.Aquamarine);
                                break;
                            }
                        default:
                            {
                                player.SendErrorMessage("Invalid item type! Available types: inv, equip, misc");
                                return;
                            }
                    }
                }
                else
                {
                    string hotbar = "|";
                    string inventory = "|";
                    for (int i = 0; i < NetItem.InventorySlots; i++)
                    {
                        if (i < 10)
                        {
                            hotbar = hotbar + "[i/s" + players[0].TPlayer.inventory[i].stack + ":" + players[0].TPlayer.inventory[i].netID + "]|";
                        }
                        else if (i < NetItem.InventorySlots)
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
                        $"[i/s{players[0].TPlayer.trashItem.stack}:{players[0].TPlayer.trashItem.netID}] (Trash)", Color.Aquamarine);
                }
            }
        }

        private void EditInventory(CommandArgs args)
        {
            var player = args.Player;

            if (args.Parameters.Count < 2)
            {
                player.SendErrorMessage("Invalid Syntax! Proper Syntax: /editinv <player> <slot> <itemID> [stack]");
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

            if (!int.TryParse(args.Parameters[1], out int slot))
            {
                player.SendErrorMessage("Invalid slot number!");
                return;
            }

            if (!int.TryParse(args.Parameters[2], out int itemID))
            {
                player.SendErrorMessage("Invalid item ID!");
                return;
            }

            if (slot < 0 || slot >= NetItem.InventorySlots)
            {
                player.SendErrorMessage("Invalid slot number! Slot must be between 0 and 49.");
                return;
            }

            var stack = 1;
            if (args.Parameters.Count >= 4)
            {
                if (!int.TryParse(args.Parameters[3], out stack))
                {
                    player.SendErrorMessage("Invalid stack number!");
                    return;
                }
                if (stack < 1 || stack > Main.maxStack)
                {
                    player.SendErrorMessage($"Stack size must be between 1 and {Main.maxStack}.");
                    return;
                }
            }

            players[0].TPlayer.inventory[slot].SetDefaults(itemID);
            players[0].TPlayer.inventory[slot].stack = stack;
            players[0].SendData(PacketTypes.PlayerSlot, "", players[0].Index, slot);
            player.SendMessage($"Modified {players[0].Name}'s inventory at slot {slot} to item ID {itemID} with stack {stack}.", Color.Aquamarine);
        }

        private void ViewProfile(CommandArgs args)
        {
            var player = args.Player;

            if (args.Parameters.Count < 1)
            {
                player.SendErrorMessage("Invalid Syntax! Proper Syntax: /profile <player>");
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

            player.SendMessage($"Viewing {players[0].Name}'s Profile:", Color.Aquamarine);
            player.SendMessage($"- HP: {players[0].TPlayer.statLife}/{players[0].TPlayer.statLifeMax}", Color.Aquamarine);
            player.SendMessage($"- Mana: {players[0].TPlayer.statMana}/{players[0].TPlayer.statManaMax}", Color.Aquamarine);
            player.SendMessage($"- Defense: {players[0].TPlayer.statDefense}", Color.Aquamarine);
            player.SendMessage($"- Damage: {players[0].TPlayer.HeldItem.damage}", Color.Aquamarine);
            player.SendMessage($"- Crit Chance: {players[0].TPlayer.meleeCrit}/{players[0].TPlayer.rangedCrit}/{players[0].TPlayer.magicCrit}/{players[0].TPlayer.thrownCrit} (Melee/Ranged/Magic/Thrown)", Color.Aquamarine);
            player.SendMessage($"- Movement Speed: {players[0].TPlayer.moveSpeed}", Color.Aquamarine);
            player.SendMessage($"- Jump Height: {players[0].TPlayer.jump}", Color.Aquamarine);
        }

        private void SetProfile(CommandArgs args)
        {
            var player = args.Player;

            if (args.Parameters.Count < 3)
            {
                player.SendErrorMessage("Invalid Syntax! Proper Syntax: /setprofile <player> <property> <value>");
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

            var property = args.Parameters[1].ToLower();
            var value = args.Parameters[2];

            switch (property)
            {
                case "hp":
                case "health":
                    if (!int.TryParse(value, out int health))
                    {
                        player.SendErrorMessage("Invalid value for health!");
                        return;
                    }
                    players[0].TPlayer.statLife = health;
                    players[0].SendData(PacketTypes.PlayerHp, "", players[0].Index);
                    player.SendMessage($"Modified {players[0].Name}'s health to {health}.", Color.Aquamarine);
                    break;

                case "mana":
                    if (!int.TryParse(value, out int mana))
                    {
                        player.SendErrorMessage("Invalid value for mana!");
                        return;
                    }
                    players[0].TPlayer.statMana = mana;
                    players[0].SendData(PacketTypes.PlayerMana, "", players[0].Index);
                    player.SendMessage($"Modified {players[0].Name}'s mana to {mana}.", Color.Aquamarine);
                    break;

                case "defense":
                    if (!int.TryParse(value, out int defense))
                    {
                        player.SendErrorMessage("Invalid value for defense!");
                        return;
                    }
                    players[0].TPlayer.statDefense = defense;
                    players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index);
                    player.SendMessage($"Modified {players[0].Name}'s defense to {defense}.", Color.Aquamarine);
                    break;

                case "damage":
                    if (!int.TryParse(value, out int damage))
                    {
                        player.SendErrorMessage("Invalid value for damage!");
                        return;
                    }
                    players[0].TPlayer.HeldItem.damage = damage;
                    players[0].SendData(PacketTypes.PlayerSlot, "", players[0].Index, players[0].TPlayer.selectedItem);
                    player.SendMessage($"Modified {players[0].Name}'s damage to {damage}.", Color.Aquamarine);
                    break;

                case "crit":
                case "critchance":
                    var critValues = value.Split('/');
                    if (critValues.Length != 4 || !int.TryParse(critValues[0], out int meleeCrit) || !int.TryParse(critValues[1], out int rangedCrit) || !int.TryParse(critValues[2], out int magicCrit) || !int.TryParse(critValues[3], out int thrownCrit))
                    {
                        player.SendErrorMessage("Invalid value for critical chance! Proper Syntax: <meleeCrit>/<rangedCrit>/<magicCrit>/<thrownCrit>");
                        return;
                    }
                    players[0].TPlayer.meleeCrit = meleeCrit;
                    players[0].TPlayer.rangedCrit = rangedCrit;
                    players[0].TPlayer.magicCrit = magicCrit;
                    players[0].TPlayer.thrownCrit = thrownCrit;
                    players[0].SendData(PacketTypes.PlayerHurtV2, "", players[0].Index);
                    player.SendMessage($"Modified {players[0].Name}'s critical chance to {meleeCrit}/{rangedCrit}/{magicCrit}/{thrownCrit} (Melee/Ranged/Magic/Thrown).", Color.Aquamarine);
                    break;

                case "movespeed":
                case "movementspeed":
                    if (!float.TryParse(value, out float moveSpeed))
                    {
                        player.SendErrorMessage("Invalid value for movement speed!");
                        return;
                    }
                    players[0].TPlayer.moveSpeed = moveSpeed;
                    players[0].SendData(PacketTypes.PlayerUpdate, "", players[0].Index);
                    player.SendMessage($"Modified {players[0].Name}'s movement speed to {moveSpeed}.", Color.Aquamarine);
                    break;

                case "jump":
                case "jumpheight":
                    if (!int.TryParse(value, out int jumpHeight))
                    {
                        player.SendErrorMessage("Invalid value for jump height!");
                        return;
                    }
                    players[0].TPlayer.jump = jumpHeight;
                    players[0].SendData(PacketTypes.PlayerUpdate, "", players[0].Index);
                    player.SendMessage($"Modified {players[0].Name}'s jump height to {jumpHeight}.", Color.Aquamarine);
                    break;

                default:
                    player.SendErrorMessage("Invalid property! Available properties: hp, mana, defense, damage, crit, movespeed, jump");
                    return;
            }
        }
    }
}
