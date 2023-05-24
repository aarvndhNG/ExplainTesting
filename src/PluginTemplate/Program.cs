using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace BossRewards
{
    [ApiVersion(2, 1)]
    public class BossRewards : TerrariaPlugin
    {
        public override string Name => "BossRewards";
        public override string Author => "YourName";
        public override string Description => "Provides configurable rewards for defeating bosses.";
        public override Version Version => new Version(1, 0, 0);

        private Dictionary<int, BossReward> bossRewards;

        public BossRewards(Main game) : base(game)
        {
            bossRewards = new Dictionary<int, BossReward>();
        }

        public override void Initialize()
        {
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
            SetupConfig();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
            }
            base.Dispose(disposing);
        }

        private void SetupConfig()
        {
            string configPath = Path.Combine(TShock.SavePath, "BossRewardsConfig.json");

            if (File.Exists(configPath))
            {
                bossRewards = JsonConvert.DeserializeObject<Dictionary<int, BossReward>>(File.ReadAllText(configPath));
            }
            else
            {
                bossRewards = CreateDefaultConfig();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(bossRewards, Formatting.Indented));
            }
        }

        private Dictionary<int, BossReward> CreateDefaultConfig()
        {
            Dictionary<int, BossReward> defaultConfig = new Dictionary<int, BossReward>();

            // Example rewards for defeating bosses
            defaultConfig[NPCID.EyeofCthulhu] = new BossReward
            {
                ItemID = ItemID.GoldCoin,
                Amount = 10,
                Message = "You received 10 gold coins for defeating the Eye of Cthulhu!"
            };

            defaultConfig[NPCID.EaterofWorldsHead] = new BossReward
            {
                ItemID = ItemID.IronSkinPotion,
                Amount = 5,
                Message = "You received 5 Ironskin Potions for defeating the Eater of Worlds!"
            };

            // Add more entries for other bosses and their respective rewards
            defaultConfig[NPCID.SkeletronHead] = new BossReward
            {
                ItemID = ItemID.ManaCrystal,
                Amount = 1,
                Message = "You received 1 Mana Crystal for defeating Skeletron!"
            };

            defaultConfig[NPCID.WallofFlesh] = new BossReward
            {
                ItemID = ItemID.WormholePotion,
                Amount = 3,
                Message = "You received 3 Wormhole Potions for defeating the Wall of Flesh!"
            };

            return defaultConfig;
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            Player player = TShock.Players[args.PlayerId];
            if (player == null || !player.Active)
                return;

            int npcType = args.npc.netID;

            if (bossRewards.ContainsKey(npcType))
            {
                BossReward reward = bossRewards[npcType];
                player.GiveItem(reward.ItemID, reward.Amount);
                player.SendSuccessMessage(reward.Message);
            }
        }
    }

    public class BossReward
    {
        public int ItemID { get; set; }
        public int Amount { get; set; }
        public string Message { get; set; }
    }
}
