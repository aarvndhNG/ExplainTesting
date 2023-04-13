const TShock = require("tshock");
const Command = TShock.Command;
const Votes = {};
const Config = require("./config.json");

function voteCommand(args, player) {
    if (args.length < 1) {
        player.SendErrorMessage("Invalid syntax! Proper syntax: /vote <yes/no>");
        return;
    }

    let vote = args[0].toLowerCase();

    if (vote !== "yes" && vote !== "no") {
        player.SendErrorMessage("Invalid vote! Please choose either 'yes' or 'no'.");
        return;
    }

    if (Votes.hasOwnProperty(player.IP)) {
        player.SendErrorMessage("You have already voted!");
        return;
    }

    Votes[player.IP] = vote;

    TShock.Utils.Broadcast($"{player.Name} has voted {vote}!", Config.VoteColor);

    let yesVotes = 0, noVotes = 0;

    for (let ip in Votes) {
        if (Votes[ip] === "yes") {
            yesVotes++;
        } else {
            noVotes++;
        }
    }

    let votePercentage = Math.round(yesVotes / (yesVotes + noVotes) * 100);

    TShock.Utils.Broadcast($"Current vote: Yes - {yesVotes}, No - {noVotes}, {votePercentage}% voted yes", Config.VoteColor);
}

const VoteCommand = new Command("vote", voteCommand, {
    usage: "/vote <yes/no>",
    help: "Starts a server-wide vote with the specified yes or no question."
});

TShock.Commands.Add(VoteCommand);
