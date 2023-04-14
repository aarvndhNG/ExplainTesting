using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace HuntingBunnyGame
{
    [ApiVersion(2, 1)]
    public class HuntingBunnyGame : TerrariaPlugin
    {
        private int bunnyCount = 0;
        private bool gameActive = false;
        private int gameWinner = -1;

        public override string Name => "Hunting Bunny Game";
        public override string Author => "Your Name";
        public override string Description => "A game where players hunt bunnies.";
        public override Version Version => new Version(1, 0, 0);

        public HuntingBunnyGame(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            LoadConfig();
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
            Commands.ChatCommands.Add(new Command("huntingbunnygame.start", StartHunting, "starthuntingbunnygame", "huntingbunnygame.start"));
            Commands.ChatCommands.Add(new Command("huntingbunnygame.end", EndHunting, "endhuntingbunnygame", "huntingbunnygame.end"));
            Commands.ChatCommands.Add(new Command("huntingbunnygame.count", ShowBunnyCount, "bunnycount", "huntingbunnygame.count"));
            Commands.ChatCommands.Add(new Command("huntingbunnygame.winner", ShowWinner, "winner", "huntingbunnygame.winner"));
        }

        private void LoadConfig()
        {
            if (File.Exists("huntingbunnygame.json"))
            {
                var config = JsonUtils.DeserializeFromFile<Config>("huntingbunnygame.json");
                bunnyCount = config.BunnyCount;
            }
            else
            {
                TShock.Log.ConsoleError("Could not find huntingbunnygame.json config file. Using default values.");
            }
        }

        private void SaveConfig()
        {
            var config = new Config { BunnyCount = bunnyCount };
            JsonUtils.SerializeToFile("huntingbunnygame.json", config);
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            if (gameActive && args.npc.type == NPCID.Bunny && args.npc.life <= 0)
            {
                bunnyCount--;
                if (bunnyCount <= 0)
                {
                    gameActive = false;
                    gameWinner = args.npc.killedByNetId;
                    TShock.Utils.Broadcast("The hunting bunny game has ended! " + TShock.Players[gameWinner].Name + " is the winner!", Color.LimeGreen);
                }
            }
        }

        private void StartHunting(CommandArgs args)
        {
            if (gameActive)
            {
                args.Player.SendErrorMessage("The hunting bunny game is already in progress!");
                return;
            }
            gameActive = true;
            gameWinner = -1;
            bunnyCount = args.TryParseParam(0, 20);
            if (bunnyCount <= 0)
            {
                bunnyCount = 20;
            }
            SaveConfig();
            TShock.Utils.Broadcast("The hunting bunny game has started! Kill " + bunnyCount + " bunnies to win!", Color.LimeGreen);
        }

        private void EndHunting(CommandArgs args)
        {
            if (!gameActive)
            {
                args.Player.SendErrorMessage("The hunting bunny game is not currently in progress!");
                return;
            }
            gameActive = false;
            TShock.Utils.Broadcast("The hunting bunny game has been ended. No winner.", Color.LimeGreen);
        }

        private void ShowBunnyCount(CommandArgs args)

        {

             args.Player.SendSuccessMessage("The current number of bunnies to kill is: " + bunnyCount);

        }
        
            private void ShowWinner(CommandArgs args)
        {
        if (gameActive)
        {
            args.Player.SendErrorMessage("The hunting bunny game is still in progress!");
            return;
        }
        if (gameWinner == -1)
        {
            args.Player.SendSuccessMessage("There was no winner in the last hunting bunny game.");
        }
        else
        {
            args.Player.SendSuccessMessage(TShock.Players[gameWinner].Name + " won the last hunting bunny game!");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
        }
        base.Dispose(disposing);
    }
}

public class Config
{
    public int BunnyCount { get; set; } = 20;

  }

}




