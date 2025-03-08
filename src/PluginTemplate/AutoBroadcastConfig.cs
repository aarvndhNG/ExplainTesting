using LazyAPI.Attributes;
using LazyAPI.ConfigFiles;

namespace AutoBroadcast;

[Config]
public class AutoBroadcastConfig : JsonConfigBase<AutoBroadcastConfig>
{
    [LocalizedPropertyName(CultureType.Chinese, "广播列表")]
    [LocalizedPropertyName(CultureType.English, "Broadcasts")]
    public Broadcast[] Broadcasts { get; set; } = Array.Empty<Broadcast>();

    protected override string Filename => "AutoBroadcast";

    protected override void SetDefault()
    {
        this.Broadcasts = new[]
        {
            new Broadcast
            {
                Name = "Sample Broadcast",
                Enabled = true,
                Messages = new [] { "/say Ciallo～(∠・ω< )⌒★", "The automatic broadcast executed the server command /say Ciallo～(∠・ω< )⌒★" },
                ColorRgb = new [] { 255, 234, 115 },
                Interval = 600,
                StartDelay = 0,
                TriggerToWholeGroup = false
            }
        };
    }
}
