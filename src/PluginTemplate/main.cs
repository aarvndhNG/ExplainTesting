using LazyAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace AutoBroadcast;

[ApiVersion(2, 1)]
public class AutoBroadcast : LazyPlugin
{
    public override string Name => System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!;

    public override string Author => "Scavenger,Cai";
    public override string Description => "Automatic Broadcast Plugin";
    
    public override Version Version => new (1, 1, 2);

    private DateTime _lastUpdate = DateTime.Now;

    public AutoBroadcast(Main game) : base(game) { }

    public override void Initialize()
    {
        ServerApi.Hooks.GameUpdate.Register(this, this.OnUpdate);
        ServerApi.Hooks.ServerChat.Register(this, OnChat, int.MinValue); // Lowest priority, so no need to handle commands
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.GameUpdate.Deregister(this, this.OnUpdate);
            ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
        }
        base.Dispose(disposing);
    }

    /*
     * Runs every second
     * Updates the timers for all broadcasts
     */
    private void OnUpdate(EventArgs args)
    {
        if (!((DateTime.Now - this._lastUpdate).TotalSeconds >= 1)) 
        {
            return;
        }
        
        this._lastUpdate = DateTime.Now;
        
        foreach (var broadcast in AutoBroadcastConfig.Instance.Broadcasts)
        {
            if (!broadcast.Enabled || broadcast.Interval == 0) // Do not update broadcasts that are not enabled or have an interval of 0
            {
                continue;
            }
            broadcast.SecondUpdate();
        }
    }

    /*
     * Chat keyword-triggered broadcasts
     * Triggers broadcasts when chat keywords are matched
     */
    private static void OnChat(ServerChatEventArgs args)
    {
        var plr = TShock.Players[args.Who];
        
        if (plr == null)
        {
            return;
        }
        
        foreach (var broadcast in AutoBroadcastConfig.Instance.Broadcasts)
        {
            if (!broadcast.Enabled)
            {
                continue;
            }
            
            if (broadcast.TriggerWords.Any(word => args.Text.Contains(word))) // Check if the message contains keywords
            {
                broadcast.RunTriggerWords(plr);
            }
        }
    }
}
