#include "pch.h"
#include "TerrariaAPI.h"
#include "tshock.h"
#include "json.hpp"
#include <fstream>

using namespace Terraria;
using json = nlohmann::json;

namespace HuntingBunnyGame
{
    int bunnyCount = 0;
    bool gameActive = false;
    int gameWinner = -1;

    void LoadConfig()
    {
        std::ifstream file("huntingbunnygame.json");
        if (file.good())
        {
            json config;
            file >> config;
            bunnyCount = config.value("bunnyCount", 20);
        }
        else
        {
            TShock::LogError("Could not find huntingbunnygame.json config file. Using default values.");
        }
    }

    void SaveConfig()
    {
        json config = {
            {"bunnyCount", bunnyCount}
        };
        std::ofstream file("huntingbunnygame.json");
        file << config.dump(4);
    }

    void BunnyKilled(PlayerDeathReason &reason, int playerId, int npcId)
    {
        if (gameActive && npcId == NPCID::Bunny && Main::npc[npcId].life <= 0)
        {
            bunnyCount--;
            if (bunnyCount <= 0)
            {
                gameActive = false;
                gameWinner = playerId;
                TShock::Broadcast("The hunting bunny game has ended! " + TShock::Players[playerId]->Name + " is the winner!");
            }
        }
    }

    void StartHunting(CommandArgs args)
    {
        if (gameActive)
        {
            TShock::Player[args.Player.Index].SendErrorMessage("The hunting bunny game is already in progress!");
            return;
        }
        gameActive = true;
        gameWinner = -1;
        bunnyCount = args.TryGetInt(0, 20);
        if (bunnyCount <= 0)
        {
            bunnyCount = 20;
        }
        SaveConfig();
        TShock::Broadcast("The hunting bunny game has started! Kill " + std::to_string(bunnyCount) + " bunnies to win!");
    }

    void EndHunting(CommandArgs args)
    {
        if (!gameActive)
        {
            TShock::Player[args.Player.Index].SendErrorMessage("The hunting bunny game is not currently in progress!");
            return;
        }
        gameActive = false;
        TShock::Broadcast("The hunting bunny game has been ended. No winner.");
    }

    void ShowBunnyCount(CommandArgs args)
    {
        if (gameActive)
        {
            TShock::Player[args.Player.Index].SendInfoMessage("Bunnies remaining: " + std::to_string(bunnyCount));
        }
        else
        {
            TShock::Player[args.Player.Index].SendInfoMessage("The hunting bunny game is not currently in progress!");
        }
    }

    void ShowWinner(CommandArgs args)
    {
        if (gameWinner != -1)
        {
            TShock::Player[args.Player.Index].SendInfoMessage("The winner of the hunting bunny game was " + TShock::Players[gameWinner]->Name + "!");
        }
        else
        {
            TShock::Player[args.Player.Index].SendInfoMessage("The hunting bunny game has not yet ended.");
        }
    }

    void Initialize(EventArgs args)
    {
        LoadConfig();
        AddPlayerDeathEvent(BunnyKilled);
        Commands.ChatCommands.Add(new Command("huntingbunnygame.start", StartHunting, "starthuntingbunnygame", "huntingbunnygame.start"));
        Commands.ChatCommands.Add(new Command("huntingbunnygame.end", EndHunting, "endhuntingbunnygame", "huntingbunnygame.end"));
        Commands.ChatCommands.Add(new Command("huntingbunnygame.bunnycount", ShowBunnyCount, "bunnycount", "huntingbunnygame.bunnycount"));
        Commands.ChatCommands.Add(new Command("huntingbunnygame.winner", ShowWinner, "winner", "huntingbunnygame.winner"));

     }

  }
