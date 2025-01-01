using System;
using System.Collections.Generic;
using System.Timers;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace TeamDeathmatch
{
    [ApiVersion(2, 1)]
    public class TeamDeathmatchPlugin : TerrariaPlugin
    {
        public override string Name => "TeamDeathmatch";
        public override string Author => "Your Name";
        public override string Description => "A team deathmatch plugin for TShock.";
        public override Version Version => new Version(1, 0, 0, 0);

        private Timer matchTimer;
        private int matchDuration = 600; // 10 minutes in seconds
        private Dictionary<int, int> teamScores;
        private Dictionary<int, Point> teamSpawns;
        private List<int> redTeam;
        private List<int> blueTeam;

        public TeamDeathmatchPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("tdm.setspawn", SetSpawn, "setspawn"));
            Commands.ChatCommands.Add(new Command("tdm.join", JoinTeam, "join"));
            Commands.ChatCommands.Add(new Command("tdm.leave", LeaveTeam, "leave"));

            teamScores = new Dictionary<int, int> { { 1, 0 }, { 2, 0 } }; // 1: Red, 2: Blue
            teamSpawns = new Dictionary<int, Point>();

            redTeam = new List<int>();
            blueTeam = new List<int>();

            matchTimer = new Timer(1000); // 1 second intervals
            matchTimer.Elapsed += OnMatchTimerElapsed;
        }

        private void SetSpawn(CommandArgs args)
        {
            if (args.Parameters.Count != 2 || !(args.Parameters[0] == "red" || args.Parameters[0] == "blue"))
            {
                args.Player.SendErrorMessage("Invalid syntax. Use /setspawn <red|blue> <x> <y>");
                return;
            }

            int x = args.Player.TileX;
            int y = args.Player.TileY;
            int team = args.Parameters[0] == "red" ? 1 : 2;
            teamSpawns[team] = new Point(x, y);
            args.Player.SendSuccessMessage($"Spawn point for {args.Parameters[0]} team set at ({x}, {y}).");
        }

        private void JoinTeam(CommandArgs args)
        {
            if (args.Parameters.Count != 1 || !(args.Parameters[0] == "red" || args.Parameters[0] == "blue"))
            {
                args.Player.SendErrorMessage("Invalid syntax. Use /join <red|blue>");
                return;
            }

            if (redTeam.Contains(args.Player.Index) || blueTeam.Contains(args.Player.Index))
            {
                args.Player.SendErrorMessage("You are already in a team.");
                return;
            }

            int team = args.Parameters[0] == "red" ? 1 : 2;
            if (team == 1 && redTeam.Count < 4)
            {
                redTeam.Add(args.Player.Index);
                args.Player.Teleport(teamSpawns[1].X * 16, teamSpawns[1].Y * 16);
                args.Player.SendSuccessMessage("You have joined the Red team!");
            }
            else if (team == 2 && blueTeam.Count < 4)
            {
                blueTeam.Add(args.Player.Index);
                args.Player.Teleport(teamSpawns[2].X * 16, teamSpawns[2].Y * 16);
                args.Player.SendSuccessMessage("You have joined the Blue team!");
            }
            else
            {
                args.Player.SendErrorMessage("The selected team is full!");
            }
        }

        private void LeaveTeam(CommandArgs args)
        {
            if (redTeam.Remove(args.Player.Index) || blueTeam.Remove(args.Player.Index))
            {
                args.Player.SendSuccessMessage("You have left your team.");
            }
            else
            {
                args.Player.SendErrorMessage("You are not part of any team.");
            }
        }

        private void OnMatchTimerElapsed(object sender, ElapsedEventArgs e)
        {
            matchDuration--;
            if (matchDuration <= 0)
            {
                EndMatch();
            }
        }

        private void EndMatch()
        {
            matchTimer.Stop();

            string winningTeam = teamScores[1] > teamScores[2] ? "Red" : "Blue";
            TSPlayer.All.SendInfoMessage($"Match over! {winningTeam} team wins with {Math.Max(teamScores[1], teamScores[2])} kills.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                matchTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
