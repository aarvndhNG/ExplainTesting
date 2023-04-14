open System.IO
open Newtonsoft.Json
open Terraria
open TerrariaApi.Server
open TShockAPI

type Config =
    { Price : int
      HouseWidth : int
      HouseHeight : int }

[<JsonObject(MemberSerialization.OptIn)>]
type HousePlugin(config : Config) =
    inherit TerrariaPlugin()
    let mutable houseWidth = config.HouseWidth
    let mutable houseHeight = config.HouseHeight
    let mutable price = config.Price

    let loadConfig() =
        let configFile = "housepluginconfig.json"
        if File.Exists(configFile) then
            let json = File.ReadAllText(configFile)
            let newConfig = JsonConvert.DeserializeObject<Config>(json)
            price <- newConfig.Price
            houseWidth <- newConfig.HouseWidth
            houseHeight <- newConfig.HouseHeight
        else
            let defaultConfig = { Price = 1000; HouseWidth = 10; HouseHeight = 8 }
            let json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented)
            File.WriteAllText(configFile, json)

    let saveConfig() =
        let config = { Price = price; HouseWidth = houseWidth; HouseHeight = houseHeight }
        let json = JsonConvert.SerializeObject(config, Formatting.Indented)
        File.WriteAllText("housepluginconfig.json", json)

    let buildHouse(player: TSPlayer) =
        let width = houseWidth
        let height = houseHeight
        let x = player.TileX - width / 2
        let y = player.TileY - height
        let id = WorldGen.KillTile x y
        if id then
            player.SendSuccessMessage("Building house...")
            WorldGen.BuildRoom x y (x + width) (y + height)
            TShock.Utils.Broadcast($"{player.Name} has built a house!", Color.LimeGreen)

    override __.Name = "HousePlugin"
    override __.Author = "Your Name"
    override __.Description = "Allows players to build houses with a single command."
    override __.Version = Version(1, 0, 0)

    override __.Initialize() =
        Commands.ChatCommands.Add new Command("house.build", buildHouse, "buildhouse")
        loadConfig() |> ignore

    override __.Unload() =
        saveConfig() |> ignore

    member __.Config =
        { Price = price
          HouseWidth = houseWidth
          HouseHeight = houseHeight }
