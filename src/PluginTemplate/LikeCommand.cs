using TShockAPI;

public class LikeCommand : TShockCommand
{
    private const int CooldownSeconds = 60; // Example cooldown

    public override void RunCommand(Player player, string[] args)
    {
        if (args.Length != 1)
        {
            player.Message("Usage: /like <player>");
            return;
        }

        Player targetPlayer = PluginManager.Instance.GetPlayer(args[0]);
        if (targetPlayer == null || !targetPlayer.IsOnline)
        {
            player.Message("Player not found.");
            return;
        }

        // Check permissions
        if (!player.HasPermission("like.give"))
        {
            player.Message("You don't have permission to use this command.");
            return;
        }

        // Check cooldown
        if (player.GetCooldown("like") > 0)
        {
            player.Message("You can only use this command once every " + CooldownSeconds + " seconds.");
            return;
        }

        // Apply the random buff
        try
        {
            BuffData randomBuff = LikePlugin.Instance.buffs[Plugin.Random.Next(LikePlugin.Instance.buffs.Count)];
            PluginManager.Instance.BuffManager.BuffPlayer(targetPlayer, randomBuff.Id, randomBuff.Duration);

            player.Message("You liked " + targetPlayer.Name + " and gave them a buff!");
            targetPlayer.Message(player.Name + " liked you and gave you a buff!");

            player.SetCooldown("like", CooldownSeconds);
        }
        catch (Exception ex)
        {
            Log.Write("Error applying buff: " + ex.Message);
            player.Message("An error occurred while applying the buff.");
        }
    }
}
