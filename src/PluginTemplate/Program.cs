using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace RandomEnemyPlugin
{
    [ApiVersion(2, 1)]
    public class RandomEnemyPlugin : TerrariaPlugin
    {
        private Config _config;

        public override string Name => "RandomEnemyPlugin";
        public override string Author => "YourName";
        public override string Description => "Spawns a random enemy.";
        public override Version Version => new Version(1, 0, 0);

        public RandomEnemyPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("spawnenemy.spawn", SpawnRandomEnemy, "spawnenemy"));
            LoadConfig();
        }

        private void LoadConfig()
        {
            _config = Config.Read();
        }

        private void SaveConfig()
        {
            _config.Write();
        }

        private void SpawnRandomEnemy(CommandArgs args)
        {
            int playerId = args.Player.Index;
            TSPlayer player = TShock.Players[playerId];
            if (player == null)
            {
                args.Player.SendErrorMessage("You are not logged in.");
                return;
            }

            var enemyTypes = _config.EnemyTypes;
            if (enemyTypes.Count == 0)
            {
                args.Player.SendErrorMessage("There are no enemy types configured.");
                return;
            }

            var random = new Random();
            var randomIndex = random.Next(0, enemyTypes.Count);
            var npcType = enemyTypes[randomIndex];

            var spawnPosition = player.TPlayer.position;
            spawnPosition.X += Main.rand.Next(-200, 200);
            spawnPosition.Y -= 300;

            string npcTypeName = parameters[0].ToLowerInvariant();
            int npcType;
            if (!int.TryParse(parameters[1], out npcType))
            {
              // handle invalid npcType parameter
                return;
            }

              // Spawn NPC
            int npcIndex = NPC.NewNPC(player.Center.X, player.Center.Y, npcType, 0, 0f, 0f, 0f, 0f, 255);
            if (npcIndex == Main.maxNPCs)
            {
              // handle failed NPC spawn
                return;
            }

             // Get the spawned NPC
            NPC npc = Main.npc[npcIndex];

             // Set the NPC's name and color
            npc.SetDefaults(npcType);
            npc.GivenName = npcTypeName;
            npc.color = Color.Red;


            TShock.Utils.Broadcast($"A {Lang.GetNPCName(npcType)} has spawned near {player.Name}!", _config.SpawnAnnouncementColor);
        }
    }

    internal class Config
    {
        public List<int> EnemyTypes { get; set; } = new List<int>();
        public string SpawnAnnouncementColor { get; set; } = "yellow";

        public static Config Read()
        {
            return new Config();
        }

        public void Write()
        {
        }
    }
}
