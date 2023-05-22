using System;
using TerrariaApi.Server;
using TShockAPI;

namespace SocialFeatures
{
    public class NpcLootEventArgs : EventArgs
    {
        public TSPlayer Player { get; set; }
        public int NpcIndex { get; set; }
        public int ItemId { get; set; }
        public int ItemStack { get; set; }
        public bool CustomLoot { get; set; }

        public NpcLootEventArgs(TSPlayer player, int npcIndex, int itemId, int itemStack, bool customLoot)
        {
            Player = player;
            NpcIndex = npcIndex;
            ItemId = itemId;
            ItemStack = itemStack;
            CustomLoot = customLoot;
        }
    }
}
