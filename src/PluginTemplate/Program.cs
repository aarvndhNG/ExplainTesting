using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace NGPlugins
{
    [ApiVersion(2, 1)]
    public class NGPlugins : TerrariaPlugin
    {
        public override string Name { get { return "NGPlugins"; } }
        public override string Author { get { return "Frontalvlad"; } }
        public override string Description { get { return "Plugin specifically for NGVille server. Shows a list of plugins."; } }
        public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        public NGPlugins(Main game)
          : base(game)
        {
        }

        public override void Initialize() => Commands.ChatCommands.Add(new Command("ngplugins", new CommandDelegate(this.ListPluginsCommand), new string[2]
        {
      "ngplugins",
      "plugins"
        }));

        private void ListPluginsCommand(CommandArgs args)
        {
            uint packedValue = Color.White.packedValue;
            string colorTag = string.Format("[c/{0:X}:", (object)((uint)(((int)packedValue & (int)byte.MaxValue) << 16 | (int)packedValue & 65280) | (packedValue & 16711680U) >> 16));
            string msg = "[i:547] [c/e3693f:Plugins]: " + string.Join("[c/ffffff:,] ", ((IEnumerable<PluginContainer>)ServerApi.Plugins).Select<PluginContainer, string>((Func<PluginContainer, string>)(p => colorTag + p.Plugin.Name.Replace("]", "]" + colorTag + "]") + "]")));
            args.Player.SendInfoMessage(msg);
        }
    }
}
