using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace PluginList
{
    [ApiVersion(2, 1)]
    public class PluginList : TerrariaPlugin
    {
        public override string Name => "Plugin List";
        public override string Author => "Your Name";
        public override string Description => "Displays a list of all loaded plugins on the server.";
        public override Version Version => new Version(1, 0, 0);

        public PluginList(Main game) : base(game) { }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("pluginlist", PluginListCommand, "pluginlist"));
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
        }

        private void PluginListCommand(CommandArgs args)
        {
            var plugins = PluginManager.Plugins;
            var pluginNames = new List<string>();

            foreach (var plugin in plugins)
            {
                if (plugin.Plugin != this)
                {
                    pluginNames.Add(plugin.Plugin.Name);
                }
            }

            args.Player.SendSuccessMessage("Plugins: " + string.Join(", ", pluginNames));
        }

        private void OnServerJoin(JoinEventArgs args)
        {
            args.Player.SendSuccessMessage("Type /pluginlist to see a list of all loaded plugins.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
            }

            base.Dispose(disposing);
        }
    }
}
