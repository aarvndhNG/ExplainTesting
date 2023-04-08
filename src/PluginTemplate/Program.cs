using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace WinterMinigame
{
    [ApiVersion(2, 1)]
    public class WinterMinigame : TerrariaPlugin
    {
        private Config _config;
        private List<TSPlayer> _players;
        private DateTime _gameStartTime;
        private DateTime _gameEndTime;

        public override string Name => "WinterMinigame";
        public override string Author => "YourName";
        public override string Description => "Winter-themed minigame.";
        public override Version Version => new Version(1, 0, 0);

        public WinterMinigame(Main game) : base(game)
        {
            Order = 2;
        }

        public override void Initialize()
        {
            _players = new List<TSPlayer>();

            Commands.ChatCommands.Add(new Command("winterminigame.start", StartGame, "startgame"));
            Commands.ChatCommands.Add(new Command("winterminigame.stop", StopGame, "stopgame"));
            Commands.ChatCommands.Add(new Command("winterminigame.status", GetGameStatus, "gamestatus"));

            LoadConfig();
            StartGame(null, new CommandArgs(TShock.Players[0], "", new string[] { }));
        }

        public override void DeInitialize()
        {
            StopGame(null, new CommandArgs(TShock.Players[0], "", new string[] { }));
            _players = null;
        }

        private void LoadConfig()
        {
            _config = Config.Read();
        }

        private void SaveConfig()
        {
            _config.Write();
        }

        private void StartGame(CommandArgs args)
        {
            if (_gameStartTime != DateTime.MinValue)
            {
                args.Player.SendErrorMessage("A game is already in progress.");
                return;
            }

            _gameStartTime = DateTime.Now;
            _gameEndTime = DateTime.Now.AddMinutes(_config.GameDuration);
            _players.Clear();

            foreach (var player in TShock.Players)
            {
                if (player != null && player.Active)
                {
                    _players.Add(player);
                    player.Teleport(_config.StartingPosition.X, _config.StartingPosition.Y);
                }
            }

            TShock.Utils.Broadcast($"A winter-themed minigame has started! Type /join to join. The game will end in {_config.GameDuration} minutes.", Color.Yellow);
        }

        private void StopGame(CommandArgs args)
        {
            if (_gameStartTime == DateTime.MinValue)
            {
                args.Player.SendErrorMessage("There is no game in progress.");
                return;
            }

            _gameStartTime = DateTime.MinValue;
            _gameEndTime = DateTime.MinValue;
            _players.Clear();

            TShock.Utils.Broadcast("The winter-themed minigame has ended.", Color.Yellow);
        }

        private void GetGameStatus(CommandArgs args)
        {
            if (_gameStartTime == DateTime.MinValue)
            {
                args.Player.SendErrorMessage("There is no game in progress.");
                return;
            }

            var timeRemaining = (_gameEndTime - DateTime.Now).ToString(@"mm\:ss");
            var message = $"There is a winter-themed minigame in progress. {_players.Count} players have joined. Time remaining: {timeRemaining}";
            args.Player.SendInfoMessage(message);
        }

        private void OnJoin(JoinEventArgs args)
        {
            if (_gameStartTime != DateTime.MinValue)
            {
                _players.Add(args.Player);
                args.Player.Teleport(_config.StartingPosition.X, _config.StartingPosition.Y);
                TShock.Utils.Broadcast($"{args.Player.Name} has joined the game.", Color.Yellow);
            }
        }

        private void OnLeave(LeaveEventArgs args)
        {
          if (_gameStartTime != DateTime.MinValue)
        {
        _players.Remove(args.Player);
        TShock.Utils.Broadcast($"{args.Player.Name} has left the game.", Color.Yellow);
        }
        }
        public override void OnInitialize()
        {
        ServerApi.Hooks.GameInitialize.Register(this, OnGameInitialize);
        ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
        ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
        }
       
       private void OnGameInitialize(EventArgs args)
        {
        Commands.ChatCommands.Add(new Command("winterminigame.join", JoinGame, "join"));
        }
       
       private void OnServerLeave(LeaveEventArgs args)
        {
          if (_gameStartTime != DateTime.MinValue)
          {
            _players.Remove(args.Player);
          }
        }
       
       private void OnGreetPlayer(GreetPlayerEventArgs args)
       {
           if (_gameStartTime != DateTime.MinValue)
          {
              args.Handled = true;
              args.Player.SendInfoMessage("A winter-themed minigame is in progress! Type /join to join.");
          }
        }
        
        private void JoinGame(CommandArgs args)
        {
            if (_gameStartTime == DateTime.MinValue)
           {
                args.Player.SendErrorMessage("There is no game in progress.");
                return;
           }
           
           if (_players.Contains(args.Player))
          {
               args.Player.SendErrorMessage("You have already joined the game.");
               return;
           }
           
           _players.Add(args.Player);
        args.Player.Teleport(_config.StartingPosition.X, _config.StartingPosition.Y);
        TShock.Utils.Broadcast($"{args.Player.Name} has joined the game.", Color.Yellow);
        }
    }
}

