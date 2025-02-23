using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

[ApiVersion(2, 1)]
public class TDMPlugin : TerrariaPlugin
{
    public override string Name => "TeamDeathMatch";
    public override string Author => "YourName";
    public override string Description => "A Team Death Match minigame for TShock";
    public override Version Version => new Version(1, 0);

    public TDMPlugin(Main game) : base(game) { }

    // Data structures to manage the game state
    private static Dictionary<string, string> playerTeams = new Dictionary<string, string>(); // PlayerName -> TeamName
    private static Dictionary<string, int> teamScores = new Dictionary<string, int>();       // TeamName -> KillCount
    private static List<string> teams = new List<string> { "Red", "Blue" };                 // Available teams
    private static Dictionary<string, Vector2> teamSpawns = new Dictionary<string, Vector2>(); // TeamName -> SpawnPosition
    private static bool gameActive = false;                                                 // Game state flag

    public override void Initialize()
    {
        // Register commands
        Commands.ChatCommands.Add(new Command(JoinTeam, "jointeam"));          // No permission required, all players can use
        Commands.ChatCommands.Add(new Command("tdm.setspawn", SetSpawn, "setspawn")); // Admin-only
        Commands.ChatCommands.Add(new Command("tdm.start", StartTDM, "starttdm"));    // Admin-only
        Commands.ChatCommands.Add(new Command("tdm.end", EndTDM, "endtdm"));          // Admin-only
        Commands.ChatCommands.Add(new Command(TDMStatus, "tdmstatus"));        // No permission required

        // Register event hooks
        GetDataHandlers.PlayerDeathEvent += OnPlayerDeath;    // Handle player deaths for scoring
        GetDataHandlers.PlayerSpawnEvent += OnPlayerSpawn;    // Handle respawns
        ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave); // Handle player disconnects
    }

    // Command: /jointeam <team>
    private void JoinTeam(CommandArgs args)
    {
        if (args.Parameters.Count != 1)
        {
            args.Player.SendErrorMessage("Usage: /jointeam <team>");
            return;
        }
        string team = args.Parameters[0];
        if (!teams.Contains(team))
        {
            args.Player.SendErrorMessage("Invalid team. Available teams: " + string.Join(", ", teams));
            return;
        }
        playerTeams[args.Player.Name] = team;
        args.Player.SendInfoMessage("You joined team " + team);
    }

    // Command: /setspawn <team> (Admin-only)
    private void SetSpawn(CommandArgs args)
    {
        if (!args.Player.HasPermission("tdm.setspawn"))
        {
            args.Player.SendErrorMessage("You don't have permission to set spawn points.");
            return;
        }
        if (args.Parameters.Count != 1)
        {
            args.Player.SendErrorMessage("Usage: /setspawn <team>");
            return;
        }
        string team = args.Parameters[0];
        if (!teams.Contains(team))
        {
            args.Player.SendErrorMessage("Invalid team.");
            return;
        }
        teamSpawns[team] = new Vector2(args.Player.TileX, args.Player.TileY);
        args.Player.SendInfoMessage("Set spawn point for team " + team);
    }

    // Command: /starttdm (Admin-only)
    private void StartTDM(CommandArgs args)
    {
        if (!args.Player.HasPermission("tdm.start"))
        {
            args.Player.SendErrorMessage("You don't have permission to start the game.");
            return;
        }
        // Ensure all teams have spawn points
        foreach (string team in teams)
        {
            if (!teamSpawns.ContainsKey(team))
            {
                args.Player.SendErrorMessage("Spawn point for team " + team + " is not set.");
                return;
            }
        }
        gameActive = true;
        teamScores.Clear();
        foreach (string team in teams)
        {
            teamScores[team] = 0;
        }
        TSPlayer.All.SendInfoMessage("Team Death Match has started!");
    }

    // Command: /endtdm (Admin-only)
    private void EndTDM(CommandArgs args)
    {
        if (!args.Player.HasPermission("tdm.end"))
        {
            args.Player.SendErrorMessage("You don't have permission to end the game.");
            return;
        }
        gameActive = false;
        TSPlayer.All.SendInfoMessage("Team Death Match has ended.");
    }

    // Command: /tdmstatus
    private void TDMStatus(CommandArgs args)
    {
        if (gameActive)
        {
            string scores = string.Join(", ", teamScores.Select(kv => $"{kv.Key}: {kv.Value}"));
            args.Player.SendInfoMessage($"Game is active. Scores: {scores}");
        }
        else
        {
            args.Player.SendInfoMessage("Game is not active.");
        }
    }

    // Event: Player death handler
    private void OnPlayerDeath(object sender, GetDataHandlers.PlayerDeathEventArgs args)
    {
        if (!gameActive)
            return;
        TSPlayer victim = args.Player;
        TSPlayer killer = args.KillerPlayer;
        if (killer != null && playerTeams.ContainsKey(killer.Name) && playerTeams.ContainsKey(victim.Name))
        {
            string killerTeam = playerTeams[killer.Name];
            string victimTeam = playerTeams[victim.Name];
            if (killerTeam != victimTeam) // Ignore friendly fire
            {
                teamScores[killerTeam]++;
                TSPlayer.All.SendInfoMessage($"Team {killerTeam} scored! Current scores: Red: {teamScores["Red"]}, Blue: {teamScores["Blue"]}");
                if (teamScores[killerTeam] >= 50)
                {
                    TSPlayer.All.SendInfoMessage($"Team {killerTeam} wins!");
                    gameActive = false;
                }
            }
        }
    }

    // Event: Player spawn handler
    private void OnPlayerSpawn(object sender, GetDataHandlers.PlayerSpawnEventArgs args)
    {
        if (!gameActive)
            return;
        TSPlayer player = TShock.Players[args.PlayerId];
        if (player != null && playerTeams.ContainsKey(player.Name))
        {
            string team = playerTeams[player.Name];
            if (teamSpawns.ContainsKey(team))
            {
                Vector2 spawn = teamSpawns[team];
                args.SpawnX = (int)spawn.X;
                args.SpawnY = (int)spawn.Y;
            }
        }
    }

    // Event: Player disconnect handler
    private void OnServerLeave(LeaveEventArgs args)
    {
        TSPlayer player = TShock.Players[args.Who];
        if (player != null && playerTeams.ContainsKey(player.Name))
        {
            playerTeams.Remove(player.Name);
        }
    }

    // Cleanup event registrations
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GetDataHandlers.PlayerDeathEvent -= OnPlayerDeath;
            GetDataHandlers.PlayerSpawnEvent -= OnPlayerSpawn;
            ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
        }
        base.Dispose(disposing);
    }
}
