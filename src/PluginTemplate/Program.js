let TShockAPI = require('tshockapi');

let vanityItems = [];

let VanityCommand = {
    name: 'vanity',
    desc: 'Sets your vanity items.',
    usage: '/vanity <slot> <item> [prefix]',
    func: setVanity,
};

function setVanity(args, player) {
    if (args.length < 2) {
        player.SendErrorMessage(`Invalid syntax! Usage: ${VanityCommand.usage}`);
        return;
    }

    let slot = parseInt(args[0]);
    let item = parseInt(args[1]);

    if (isNaN(slot) || isNaN(item) || slot < 0 || slot > 2 || item < 0 || item >= TShockAPI.Utils.ItemMax) {
        player.SendErrorMessage('Invalid slot or item ID!');
        return;
    }

    let prefix = '';
    if (args.length > 2) {
        prefix = args.slice(2).join(' ');
    }

    vanityItems[slot] = { NetId: item, Prefix: prefix };

    player.SendSuccessMessage(`Vanity slot ${slot} set to item ${item} with prefix "${prefix}".`);
    player.SendData(PacketTypes.PlayerInfo, "", player.Index);
}

function onJoin(args) {
    let player = TShockAPI.Players[args.Who];
    player.SetData('vanity', vanityItems);
}

function onPlayerData(args) {
    let player = TShockAPI.Players[args.Msg.whoAmI];

    if (args.MsgID == PacketTypes.PlayerInventory) {
        let inventory = TShockAPI.DeserializeInventory(args.Msg.readBuffer, args.Msg.readBuffer.Length);
        if (inventory != null) {
            for (let i = 0; i < 3; i++) {
                let item = inventory.armor[i + 10];
                if (vanityItems[i] != null) {
                    item.netDefaults(vanityItems[i].NetId);
                    if (vanityItems[i].Prefix != '') {
                        item.Prefix(vanityItems[i].Prefix);
                    }
                }
            }
            inventory.armor = inventory.armor.slice(0, 10 + 3).concat(inventory.armor.slice(10 + 6));
            player.SendData(PacketTypes.PlayerInventory, "", player.Index, inventory.Serialize());
        }
    }
}

function onLogout(args) {
    let player = TShockAPI.Players[args.Who];
    player.SetData('vanity', null);
}

function onInitialize() {
    TShockAPI.Commands.ChatCommands.push(VanityCommand);
    TShockAPI.Hooks.ServerHooks.Join.register(onJoin);
    TShockAPI.Hooks.NetHooks.GetData.Register(PacketTypes.PlayerData, onPlayerData);
    TShockAPI.Hooks.ServerHooks.Leave.register(onLogout);
}

module.exports = {
    init: onInitialize,
};
