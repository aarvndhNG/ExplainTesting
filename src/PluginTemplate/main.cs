using System;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TShockAPI;

public class LikePlugin : TShockPlugin
{
    public override void OnEnable()
    {
        PluginManager.Instance.RegisterCommand(
            "like",
            this,
            typeof(LikeCommand),
            "Give a random buff to a player",
            "/like <player>"
        );

        LoadBuffsFromConfig();
    }

    private List<BuffData> buffs = new List<BuffData>();

    private void LoadBuffsFromConfig()
    {
        string configPath = Server.Instance.FilePath + "/LikeBuffs.json"; // Adjust the path as needed
        try
        {
            using (StreamReader reader = new StreamReader(configPath))
            {
                string json = reader.ReadToEnd();
                List<BuffData> parsedBuffs = JsonConvert.DeserializeObject<List<BuffData>>(json); // Assuming JSON format
                buffs.AddRange(parsedBuffs);
            }
        }
        catch (Exception ex)
        {
            Log.Write("Error loading buffs: " + ex.Message);
        }
    }
}
