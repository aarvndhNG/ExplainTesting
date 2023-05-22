using Terraria;
using TerrariaApi.Server;

namespace NpcLootEventArgs
{
    public class NpcLootEventArgs : TerrariaApi.Server.NpcLootEventArgs
    {
        public NpcLootEventArgs(NPC npc, int whoAmI) : base(npc, whoAmI)
        {
        }
    }
}
