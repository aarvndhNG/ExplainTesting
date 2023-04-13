import clr
import re

clr.AddReference("TerrariaServer")
from TerrariaServer import *

class Mention:
    def __init__(self):
        self.config = TShockAPI.Config.ConfigurationFile("mention.config", True)
        self.config.Settings.Add("Enabled", True)
        self.config.Settings.Add("MentionTag", "[{player}]")
        self.config.Settings.Add("MentionTagColor", "yellow")
        self.config.Save()
        
        ServerApi.Hooks.ServerChat.Register(self, self.OnChat)
        TShockAPI.Commands.ChatCommands.Add(new_command("mention", self.MentionPlayer, "mention <player>", "Mentions a player by name."))
        
    def OnChat(self, args):
        if not self.config.Settings.Enabled:
            return
            
        for player in TShock.Players:
            if player and player.Active and player.Name != args.PlayerName:
                # Match the pattern "@playername" in the chat message
                pattern = r"@" + re.escape(player.Name)
                match = re.search(pattern, args.Text)

                if match:
                    # Replace the "@playername" with the configured mention tag
                    mention = self.config.Settings.MentionTag.replace("{player}", player.Name)
                    mention = TShockAPI.Utils.ParseMessage(mention, self.config.Settings.MentionTagColor)
                    text = re.sub(pattern, mention, args.Text)

                    # Send the modified message to the player who sent it
                    player.SendSuccessMessage("<" + args.PlayerName + "> " + text)

                    # Cancel the chat event to prevent the original message from being sent
                    args.Handled = True
                    return

    def MentionPlayer(self, args, player):
        if not args:
            player.SendErrorMessage("Usage: /mention <player>")
            return

        target_name = args[0]

        for target in TShock.Players:
            if target and target.Active and target.Name == target_name:
                # Replace the mention tag with the configured tag
                mention = self.config.Settings.MentionTag.replace("{player}", target.Name)
                mention = TShockAPI.Utils.ParseMessage(mention, self.config.Settings.MentionTagColor)

                # Send the mention message to the player who used the command
                player.SendSuccessMessage("You mentioned " + mention)

                # Send the mention message to the targeted player
                target.SendSuccessMessage(player.Name + " mentioned you " + mention)

                return

        player.SendErrorMessage("Player not found: " + target_name)

Mention()
