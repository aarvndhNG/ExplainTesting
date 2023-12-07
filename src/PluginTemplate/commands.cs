using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using TShockAPI;
using ZombieModePlus.Utils;

namespace ZombieModePlus.Commands
{
    public static class AdminCommands
    {
        public static void HandleCommand(CommandArgs args)
        {
            switch (args.Parameters[0].ToLower())
            {
                case "create room":
                    if (args.Parameters.Length < 3)
                    {
                        args.Player.SendErrorMessage("Usage: /zma create room (name)");
                        return;
                    }

                    string roomName = args.Parameters[2];

                    if (RoomManager.CreateRoom(roomName))
                    {
                        args.Player.SendSuccessMessage("Room '" + roomName + "' created successfully.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Failed to create room.");
                    }

                    break;
                case "remove room":
                    if (args.Parameters.Length < 2)
                    {
                        args.Player.SendErrorMessage("Usage: /zma remove room (id)");
                        return;
                    }

                    int roomId;
                    if (!int.TryParse(args.Parameters[1], out roomId))
                    {
                        args.Player.SendErrorMessage("Invalid room ID.");
                        return;
                    }

                    if (RoomManager.RemoveRoom(roomId))
                    {
                        args.Player.SendSuccessMessage("Room ID " + roomId + " removed successfully.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Room ID " + roomId + " not found.");
                    }

                    break;
                case "start room":
                    if (args.Parameters.Length < 2)
                    {
                        args.Player.SendErrorMessage("Usage: /zma start room (id)");
                        return;
                    }

                    if (!int.TryParse(args.Parameters[1], out roomId))
                    {
                        args.Player.SendErrorMessage("Invalid room ID.");
                        return;
                    }

                    if (GameManager.StartGame(roomId))
                    {
                        args.Player.SendSuccessMessage("Game in room " + roomId + " started.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Failed to start game in room " + roomId + ".");
                    }

                    break;
                case "stop room":
                    if (args.Parameters.Length < 2)
                    {
                        args.Player.SendErrorMessage("Usage: /zma stop room (id)");
                        return;
                    }

                    if (!int.TryParse(args.Parameters[1], out roomId))
                    {
                        args.Player.SendErrorMessage("Invalid room ID.");
                        return;
                    }

                    if (GameManager.StopGame(roomId))
                    {
                        args.Player.SendSuccessMessage("Game in room " + roomId + " stopped.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Game in room " + roomId + " not found.");
                    }

                    break;
                case "newpack":
                    if (args.Parameters.Length < 3)
                    {
                        args.Player.SendErrorMessage("Usage: /zma newpack (name)");
                        return;
                    }

                    string packName = args.Parameters[2];

                    if (PackManager.CreatePack(packName))
                    {
                        args.Player.SendSuccessMessage("Pack '" + packName + "' created successfully.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Failed to create pack.");
                    }

                    break;
                case "list packs":
                    args.Player.SendInfoMessage("Available packs:");
                    foreach (Pack pack in PackManager.GetPacks())
                    {
                        args.Player.SendInfoMessage(" - " + pack.Name);
                    }
                    break;
                case "setpack room":
                    if (args.Parameters.Length < 4)
                    {
                        args.Player.SendErrorMessage("Usage: /zma setpack room (id) (pack)");
                        return;
                    }

                    if (!int.TryParse(args.Parameters[1], out roomId))
                    {
                        args.Player.SendErrorMessage("Invalid room ID.");
                        return;
                    }

                    string packName = args.Parameters[3];

                    if (RoomManager.SetPack(roomId, packName))
                    {
                        args.Player.SendSuccessMessage("Pack for");
                        return;
                    }
                    case "sgt room":
    if (args.Parameters.Length < 4)
    {
        args.Player.SendErrorMessage("Usage: /zma sgt room (id) (time)");
        return;
    }

    if (!int.TryParse(args.Parameters[1], out roomId))
    {
        args.Player.SendErrorMessage("Invalid room ID.");
        return;
    }

    int gameTime;
    if (!int.TryParse(args.Parameters[3], out gameTime))
    {
        args.Player.SendErrorMessage("Invalid game time.");
        return;
    }

    if (RoomManager.SetGameTime(roomId, gameTime))
    {
        args.Player.SendSuccessMessage("Game time for room " + roomId + " set to " + gameTime + " seconds.");
    }
    else
    {
        args.Player.SendErrorMessage("Failed to set game time.");
    }

    break;
case "swt room":
    if (args.Parameters.Length < 5)
    {
        args.Player.SendErrorMessage("Usage: /zma swt room (id) (type) (value)");
        return;
    }

    if (!int.TryParse(args.Parameters[1], out roomId))
    {
        args.Player.SendErrorMessage("Invalid room ID.");
        return;
    }

    string winTriggerType = args.Parameters[3];
    if (winTriggerType != "KillAllHumans" && winTriggerType != "TimeLimit")
    {
        args.Player.SendErrorMessage("Invalid win trigger type. Valid options: KillAllHumans, TimeLimit");
        return;
    }

    int winTriggerValue;
    if (!int.TryParse(args.Parameters[4], out winTriggerValue))
    {
        args.Player.SendErrorMessage("Invalid win trigger value.");
        return;
    }

    if (RoomManager.SetWinTrigger(roomId, winTriggerType, winTriggerValue))
    {
        if (winTriggerType == "KillAllHumans")
        {
            args.Player.SendSuccessMessage("Win trigger for room " + roomId + " set to kill " + winTriggerValue + " humans.");
        }
        else
        {
            args.Player.SendSuccessMessage("Win trigger for room " + roomId + " set to reach time limit of " + winTriggerValue + " seconds.");
        }
    }
    else
    {
        args.Player.SendErrorMessage("Failed to set win trigger.");
    }

    break;
case "sst room":
    if (args.Parameters.Length < 4)
    {
        args.Player.SendErrorMessage("Usage: /zma sst room (id) (time)");
        return;
    }

    if (!int.TryParse(args.Parameters[1], out roomId))
    {
        args.Player.SendErrorMessage("Invalid room ID.");
        return;
    }

    DateTime startTime;
    if (!DateTime.TryParse(args.Parameters[3], out startTime))
    {
        args.Player.SendErrorMessage("Invalid start time format. Use format YYYY-MM-DD HH:MM:SS.");
        return;
    }

    if (RoomManager.SetStartTime(roomId, startTime))
    {
        args.Player.SendSuccessMessage("Start time for room " + roomId + " set to " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + ".");
    }
    else
    {
        args.Player.SendErrorMessage("Failed to set start time.");
    }

    break;
case "srp room":
    if (args.Parameters.Length < 5)
    {
        args.Player.SendErrorMessage("Usage: /zma srp room (id) (x) (y)");
        return;
    }

    if (!int.TryParse(args.Parameters[1], out roomId))
    {
        args.Player.SendErrorMessage("Invalid room ID.");
        return;
    }

    int x;
    if (!int.TryParse(args.Parameters[3], out x))
    {
        args.Player.SendErrorMessage("Invalid x coordinate.");
        return;
    }

    int y;
    if (!int.TryParse(args.Parameters[4], out y))
    {
        args.Player.SendErrorMessage("Invalid y coordinate.");
        return;
    }

if (RoomManager.SetRespawnPoint(roomId, x, y))
{
    args.Player.SendSuccessMessage("Respawn point for room " + roomId + " set to (" + x + ", " + y + ").");
}
else
{
    args.Player.SendErrorMessage("Failed to set respawn point.");
}

break;
case "snp room":
    if (args.Parameters.Length < 3)
    {
        args.Player.SendErrorMessage("Usage: /zma snp room (id) (name)");
        return;
    }

    if (!int.TryParse(args.Parameters[1], out roomId))
    {
        args.Player.SendErrorMessage("Invalid room ID.");
        return;
    }

    string packName = args.Parameters[3];

    if (RoomManager.SetPack(roomId, packName))
    {
        args.Player.SendSuccessMessage("Pack for room " + roomId + " set to " + packName + ".");
    }
    else
    {
        args.Player.SendErrorMessage("Failed to set pack.");
    }

    break;
case "shp room":
    if (args.Parameters.Length < 4)
    {
        args.Player.SendErrorMessage("Usage: /zma shp room (id) (health)");
        return;
    }

    if (!int.TryParse(args.Parameters[1], out roomId))
    {
        args.Player.SendErrorMessage("Invalid room ID.");
        return;
    }

    int health;
    if (!int.TryParse(args.Parameters[3], out health))
    {
        args.Player.SendErrorMessage("Invalid health value.");
        return;
    }

    if (RoomManager.SetPlayerHealth(roomId, health))
    {
        args.Player.SendSuccessMessage("Player health for room " + roomId + " set to " + health + ".");
    }
    else
    {
        args.Player.SendErrorMessage("Failed to set player health.");
    }

    break;
case "hp room":
    if (args.Parameters.Length < 3)
    {
        args.Player.SendErrorMessage("Usage: /zma hp room (id) (amount)");
        return;
    }

    if (!int.TryParse(args.Parameters[1], out roomId))
    {
        args.Player.SendErrorMessage("Invalid room ID.");
        return;
    }

    int healAmount;
    if (!int.TryParse(args.Parameters[3], out healAmount))
    {
        args.Player.SendErrorMessage("Invalid heal amount.");
        return;
    }

    GameManager.HealPlayers(roomId, healAmount);
    args.Player.SendSuccessMessage("Healed players in room " + roomId + " by " + healAmount + " health points.");

    break;
case "svp room":
    if (args.Parameters.Length < 4)
    {
        args.Player.SendErrorMessage("Usage: /zma svp room (id) (on/off)");
        return;
    }

    if (!int.TryParse(args.Parameters[1], out roomId))
    {
        args.Player.SendErrorMessage("Invalid room ID.");
        return;
    }

    bool visible = args.Parameters[3].Equals("on", StringComparison.OrdinalIgnoreCase);

    if (RoomManager.SetPlayerVisibility(roomId, visible))
    {
        if (visible)
        {
            args.Player.SendSuccessMessage("Player visibility for room " + roomId + " enabled.");
        }
        else
        {
            args.Player.SendSuccessMessage("Player visibility for room " + roomId + " disabled.");
        }
    }
    else
    {
        args.Player.SendErrorMessage("Failed to set player visibility.");
    }

    break;
case "info room":
    if (args.Parameters.Length < 2)
    {
        args.Player.SendErrorMessage("Usage: /zm info room (id)");
        return;
    }

    if (!int.TryParse(args.Parameters[1], out roomId))
    {
        args.Player.SendErrorMessage("Invalid room ID.");
        return;
    }

    if (!RoomManager.TryGetRoom(roomId, out Room room))
    {
        args.Player.Send

