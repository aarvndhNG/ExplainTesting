using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace MyPlugin
{
    [ApiVersion(2, 1)]
    public class MyPlugin : TerrariaPlugin
    {
        private int _number;
        private bool _gameStarted;
        private List<TSPlayer> _players;

        public override string Name => "MyPlugin";
        public override string Author => "Your Name";
        public override string Description => "A custom TShock plugin.";
        public override Version Version => new Version(1, 0, 0);

        public MyPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            _players = new List<TSPlayer>();
            Commands.ChatCommands.Add(new Command("myplugin.guess", Guess, "guess"));
        }

        private void Guess(CommandArgs args)
        {
            if (!_gameStarted)
            {
                _number = new Random().Next(1, 101);
                _gameStarted = true;
                _players.Clear();
                args.Player.SendSuccessMessage("Guess the number between 1 and 100!");
            }

            if (!_players.Contains(args.Player))
            {
                _players.Add(args.Player);
            }

            int guess;
            if (!int.TryParse(args.Parameters[0], out guess))
            {
                args.Player.SendErrorMessage("Invalid guess!");
                return;
            }

            if (guess < 1 || guess > 100)
            {
                args.Player.SendErrorMessage("Guess must be between 1 and 100!");
                return;
            }

            if (guess == _number)
            {
                _gameStarted = false;
                foreach (var player in _players)
                {
                    player.SendSuccessMessage("Congratulations! {0} guessed the number {1} and won the game!", args.Player.Name, _number);
                }
            }
            else
            {
                args.Player.SendSuccessMessage("Your guess is {0}.", guess);
            }
        }
    }
}
