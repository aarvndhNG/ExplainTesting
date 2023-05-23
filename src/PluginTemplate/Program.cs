using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace EconomySystem
{
    [ApiVersion(2, 1)]
    public class EconomySystem : TerrariaPlugin
    {
        public override string Name => "EconomySystem";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "YourName";
        public override string Description => "In-game economy system with currency, trading, and shops.";

        private Dictionary<string, int> playerBalances;
        private Dictionary<string, List<Item>> playerInventory;

        public EconomySystem(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            playerBalances = new Dictionary<string, int>();
            playerInventory = new Dictionary<string, List<Item>>();

            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            TShockAPI.Commands.ChatCommands.Add(new Command("economy", EconomyCommand, "economy"));

            LoadData();
        }

        public override void DeInitialize()
        {
            SaveData();
        }

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    var playerID = reader.ReadInt16();
                    var hitDirection = reader.ReadByte();
                    var deathDamage = reader.ReadInt16();
                    var deathSource = reader.ReadByte();
                    var playerIdAssist = reader.ReadByte();

                    if (playerIdAssist != playerID)
                    {
                        var player = TShock.Players[playerID];
                        if (player != null)
                        {
                            var playerName = player.Name;
                            if (playerBalances.ContainsKey(playerName))
                            {
                                var penaltyAmount = CalculatePenaltyAmount(deathDamage);
                                playerBalances[playerName] -= penaltyAmount;
                                player.SendInfoMessage($"You lost {penaltyAmount} currency due to death.");
                            }
                        }
                    }
                }
            }
        }

        private void EconomyCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /economy <balance|pay|shop>");
                return;
            }

            var subCommand = args.Parameters[0];

            switch (subCommand.ToLower())
            {
                case "balance":
                    CheckBalance(args.Player);
                    break;

                case "pay":
                    if (args.Parameters.Count < 3)
                    {
                        args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /economy pay <player> <amount>");
                        return;
                    }

                    var recipient = args.Parameters[1];
                    var amountStr = args.Parameters[2];

                    if (!playerBalances.ContainsKey(args.Player.Name))
                    {
                        args.Player.SendErrorMessage("You don't have a valid balance.");
                        return;
                    }

                    if (!playerBalances.ContainsKey(recipient))
                    {
                        args.Player.SendErrorMessage($"'{recipient}' does not have a valid balance.");
                        return;
                    }

                    if (!int.TryParse(amountStr, out var amount) || amount <= 0)
                    {
                        args.Player.SendErrorMessage("Invalid amount specified.");
                        return;
                    }

                    if (playerBalances[args.Player.Name] < amount)
                    {
                        args.Player.SendErrorMessage("You don't have enough currency.");
                        return;
                    }

                    playerBalances[args.Player.Name] -= amount;
                    playerBalances[recipient] += amount;

                    args.Player.SendSuccessMessage($"You paid {recipient} {amount} currency.");
                    TShock.Players.SendMessage($"{args.Player.Name} paid you {amount} currency.", recipient, Color.Yellow);
                    break;

                case "shop":
                    // Implement shop functionality here
                    break;

                default:
                    args.Player.SendErrorMessage("Invalid subcommand! Available subcommands: balance, pay, shop");
                    break;
            }
        }

        private void CheckBalance(TSPlayer player)
        {
            if (playerBalances.ContainsKey(player.Name))
                player.SendInfoMessage($"Your balance: {playerBalances[player.Name]} currency");
            else
                player.SendErrorMessage("You don't have a valid balance.");
        }

        private int CalculatePenaltyAmount(int deathDamage)
        {
            // Implement your own calculation logic here for determining the penalty amount
            return deathDamage / 10;
        }

        private void SaveData()
        {
            var data = new Dictionary<string, object>
            {
                { "Balances", playerBalances },
                { "Inventory", playerInventory }
            };

            File.WriteAllText("economy.json", JsonConvert.SerializeObject(data));
        }

        private void LoadData()
        {
            if (File.Exists("economy.json"))
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText("economy.json"));

                if (data != null)
                {
                    if (data.TryGetValue("Balances", out var balances))
                        playerBalances = balances as Dictionary<string, int>;

                    if (data.TryGetValue("Inventory", out var inventory))
                        playerInventory = inventory as Dictionary<string, List<Item>>;
                }
            }
        }
    }
}

