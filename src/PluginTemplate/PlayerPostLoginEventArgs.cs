using Terraria;
using TerrariaApi.Server;

namespace PlayerPostLoginEventArgs
{
    public class PlayerPostLoginEventArgs : TerrariaApi.Server.PlayerPostLoginEventArgs
    {
        public PlayerPostLoginEventArgs(TSPlayer player) : base(player)
        {
        }
    }
}
