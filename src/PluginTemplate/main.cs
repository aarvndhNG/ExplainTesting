using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace ZombieSurvival
{
    [ApiVersion(2, 1)]
    public class ZombieSurvival : TerrariaPlugin
    {
        public static Dictionary<int, int> playerZombieKills = new Dictionary<int, int>();
        public static Dictionary<int, int> playerZombieDeaths = new Dictionary<int, int>();
        public static Dictionary<int, int> playerScore = new Dictionary<int, int>();

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnGameInitialize);
            ServerApi.Hooks.PlayerJoin.Register(this, OnPlayerJoin);
            ServerApi.Hooks.PlayerLeave.Register(this, OnPlayerLeave);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
            ServerApi.Hooks.Command.Register(this, "ZombieKills", OnZombieKillsCommand);
            ServerApi.Hooks.Command.Register(this, "ZombieDeaths", OnZombieDeathsCommand);
            ServerApi.Hooks.Command.Register(this, "PlayerScore", OnPlayerScoreCommand);
            ServerApi.Hooks.Command.Register(this, "StartZombieSurvival", OnStartZombieSurvivalCommand);
        }

        private void OnGameInitialize(EventArgs args)
        {
            // Initialize any game-wide data here
        }

        private void OnPlayerJoin(TShockAPI.Hooks.PlayerJoinEventArgs args)
        {
            // Initialize player-specific data here
            playerZombieKills[args.Player.Index] = 0;
            playerZombieDeaths[args.Player.Index] = 0;
            playerScore[args.Player.Index] = 0;
        }

        private void OnPlayerLeave(TShockAPI.Hooks.PlayerLeaveEventArgs args)
        {
            // Clean up player-specific data here
            playerZombieKills.Remove(args.Player.Index);
            playerZombieDeaths.Remove(args.Player.Index);
            playerScore.Remove(args.Player.Index);
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            if (args.NPC.type == NPCID.Zombie)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 10;
            }
            else if (args.NPC.type == NPCID.ZombieEskimo)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 20;
            }
            else if (args.NPC.type == NPCID.ZombieViking)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 30;
            }
            else if (args.NPC.type == NPCID.ZombieWerewolf)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 40;
            }
            else if (args.NPC.type == NPCID.ZombieBee)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 50;
            }
            else if (args.NPC.type == NPCID.ZombieBuffalo)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 100;
            }
            else if (args.NPC.type == NPCID.ZombieMerman)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 150;
            }
            else if (args.NPC.type == NPCID.ZombieMerchant)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 200;
            }
            else if (args.NPC.type == NPCID.ZombieChef)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 250;
            }
            else if (args.NPC.type == NPCID.ZombieDoctor)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 300;
            }
            else if (args.NPC.type == NPCID.ZombieFisherman)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 350;
            }
            else if (args.NPC.type == NPCID.ZombieGoblin)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 400;
            }
            else if (args.NPC.type == NPCID.ZombieGiant)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 500;
            }
            else if (args.NPC.type == NPCID.ZombieGunner)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 600;
            }
            else if (args.NPC.type == NPCID.ZombieMartian)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 700;
            }
            else if (args.NPC.type == NPCID.ZombieNinja)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 800;
            }
            else if (args.NPC.type == NPCID.ZombiePirate)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 900;
            }
            else if (args.NPC.type == NPCID.ZombieSanta)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 1000;
            }
            else if (args.NPC.type == NPCID.ZombieSkeleton)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 1100;
            }
            else if (args.NPC.type == NPCID.ZombieWitch)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 1200;
            }
            else if (args.NPC.type == NPCID.ZombieWizard)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 1300;
            }
            else if (args.NPC.type == NPCID.ZombieWyvern)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 1400;
            }
            else if (args.NPC.type == NPCID.ZombieYeti)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 1500;
            }
            else if (args.NPC.type == NPCID.ZombieBoss)
            {
                int playerIndex = args.WhoKilled.Index;
                playerZombieKills[playerIndex]++;
                playerScore[playerIndex] += 2000;
            }
        }
        private void OnZombieKillsCommand(CommandEventArgs args)
    {  
           if (!args.Player.HasPermission("zombiesurvival.command.zombiekills"))
    {
        args.Player.SendErrorMessage("You do not have permission to use this command.");
        return;
    }

    if (args.Parameters.Count < 1)
    {
        args.Player.SendErrorMessage("Usage: /zombiekills [player]");
        return;
    }

    var targetPlayer = TShock.Utils.FindPlayer(args.Parameters[0]);

    if (targetPlayer == null)
    {
        args.Player.SendErrorMessage("Player not found.");
        return;
    }

    var kills = playerZombieKills[targetPlayer.Index];
    args.Player.SendSuccessMessage($"{targetPlayer.Name} has killed {kills} zombies.");
}

private void OnZombieDeathsCommand(CommandEventArgs args)
{
    if (!args.Player.HasPermission("zombiesurvival.command.zombiedeaths"))
    {
        args.Player.SendErrorMessage("You do not have permission to use this command.");
        return;
    }

    if (args.Parameters.Count < 1)
    {
        args.Player.SendErrorMessage("Usage: /zombiedeaths [player]");
        return;
    }

    var targetPlayer = TShock.Utils.FindPlayer(args.Parameters[0]);

    if (targetPlayer == null)
    {
        args.Player.SendErrorMessage("Player not found.");
        return;
    }

    var deaths = playerZombieDeaths[targetPlayer.Index];
    args.Player.SendSuccessMessage($"{targetPlayer.Name} has died {deaths} times to zombies.");
}

private void OnPlayerScoreCommand(CommandEventArgs args)
{
    if (!args.Player.HasPermission("zombiesurvival.command.playerscore"))
    {
        args.Player.SendErrorMessage("You do not have permission to use this command.");
        return;
    }

    if (args.Parameters.Count < 1)
    {
        args.Player.SendErrorMessage("Usage: /playerscore [player]");
        return;
    }

    var targetPlayer = TShock.Utils.FindPlayer(args.Parameters[0]);

    if (targetPlayer == null)
    {
        args.Player.SendErrorMessage("Player not found.");
        return;
    }

    var score = playerScore[targetPlayer.Index];
    args.Player.SendSuccessMessage($"{targetPlayer.Name}'s score is {score}.");
}
private void OnStartZombieSurvivalCommand(CommandEventArgs args)
{
    if (!args.Player.HasPermission("zombiesurvival.command.startzombiesurvival"))
    {
        args.Player.SendErrorMessage("You do not have permission to use this command.");
        return;
    }

    if (args.Parameters.Count < 1)
    {
        args.Player.SendErrorMessage("Usage: /startzombiesurvival [duration]");
        return;
    }

    if (!int.TryParse(args.Parameters[0], out var duration))
    {
        args.Player.SendErrorMessage("Invalid duration.");
        return;
    }

    TShock.Players.Broadcast("Zombie survival minigame starting in 10 seconds!", Color.Yellow);
    TShock.Utils.Broadcast("Zombie survival minigame starting in 10 seconds!", 10);
    TShock.Utils.Broadcast("Zombie survival minigame starting in 5 seconds!", 5);
    TShock.Utils.Broadcast("Zombie survival minigame starting in 3...2...1...GO!", 3);

    TShock.Scheduler.Schedule(TimeSpan.FromSeconds(10), () =>
    {
        TShock.Players.Broadcast("Zombie survival minigame has started!", Color.Yellow);
        TShock.Scheduler.Schedule(TimeSpan.FromMinutes(duration), () =>
        {
            TShock.Players.Broadcast("Zombie survival minigame has ended!", Color.Yellow);
            TShock.Utils.Broadcast("Zombie survival minigame has ended!", 10);

            int maxScore = playerScore.Values.Max();
            int winnerIndex = playerScore.FirstOrDefault(p => p.Value == maxScore).Key;

            if (winnerIndex != -1)
            {
                var winner = TShock.Players[winnerIndex];
                TShock.Players.Broadcast($"{winner.Name} is the winner with a score of {maxScore}!", Color.Yellow);
                TShock.Utils.Broadcast($"{winner.Name} is the winner with a score of {maxScore}!", 10);
            }
            else
            {
                TShock.Players.Broadcast("No winner this time. Better luck next time!", Color.Yellow);
                TShock.Utils.Broadcast("No winner this time. Better luck next time!", 10);
            }

            playerZombieKills.Clear();
            playerZombieDeaths.Clear();
            playerScore.Clear();
        });
    });
}
