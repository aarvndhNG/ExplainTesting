public class ZombieMinigame implements CommandExecutor {
    
    private final TShockAPI api;
    private final Random random;
    private final Map<Integer, Integer> playerScores;
    private boolean isRunning;
    private int waveCount;
    
    public ZombieMinigame(TShockAPI api) {
        this.api = api;
        this.random = new Random();
        this.playerScores = new HashMap<>();
        this.isRunning = false;
        this.waveCount = 0;
    }
    
    @Override
    public void execute(CommandArgs args) {
        Player player = args.getPlayer();
        if (args.TryPop("start")) {
            startGame(player);
        } else if (args.TryPop("stop")) {
            stopGame(player);
        } else if (args.TryPop("score")) {
            showScore(player);
        } else {
            player.SendErrorMessage("Invalid syntax! Use /zombie start|stop|score.");
        }
    }
    
    private void startGame(Player player) {
        if (isRunning) {
            player.SendErrorMessage("A game is already in progress!");
            return;
        }
        
        isRunning = true;
        waveCount = 1;
        api.TSPlayer.All.SendInfoMessage("Zombie minigame has started! Wave 1 incoming!");
        
        api.TSPlayer.All.SpawnNPC(NPCType.Zombie, 0, player.TileX, player.TileY);
        api.TSPlayer.All.SpawnNPC(NPCType.Zombie, 0, player.TileX + 5, player.TileY);
        api.TSPlayer.All.SpawnNPC(NPCType.Zombie, 0, player.TileX - 5, player.TileY);
        
        api.TSPlayer.Server.SetTime(false, 0.0);
        api.TSPlayer.Server.SetTime(false, 0.25);
    }
    
    private void stopGame(Player player) {
        if (!isRunning) {
            player.SendErrorMessage("No game is currently in progress!");
            return;
        }
        
        isRunning = false;
        api.TSPlayer.All.SendInfoMessage("Zombie minigame has ended!");
        playerScores.clear();
    }
    
    private void showScore(Player player) {
        if (playerScores.isEmpty()) {
            player.SendErrorMessage("No scores have been recorded yet!");
            return;
        }
        
        StringBuilder builder = new StringBuilder();
        builder.append("Zombie minigame scores:\n");
        for (Map.Entry<Integer, Integer> entry : playerScores.entrySet()) {
            builder.append(api.TSPlayer.FindByID(entry.getKey()).Name)
                .append(": ")
                .append(entry.getValue())
                .append("\n");
        }
        player.SendSuccessMessage(builder.toString());
    }
    
    @EventHandler(priority = EventPriority.Normal)
    public void onNPCSpawn(NPCSpawnEvent event) {
        if (!isRunning || event.NPCType != NPCType.Zombie) {
            return;
        }
        
        event.NPC.TownNPC = true;
        event.NPC.homeless = true;
        event.NPC.aiStyle = -1;
        event.NPC.life = 50 * waveCount;
        event.NPC.lifeMax = 50 * waveCount;
        event.NPC.damage = 10 * waveCount;
    }
    
    @EventHandler(priority = EventPriority.Normal)
    public void onNPCDeath(NPCDeathEvent event) {
        if (!isRunning || event.NPCType != NPCType.Zombie) {
            return;
        }
        
        TSPlayer killer = event.Player;
        if (killer != null) {
            int score = playerScores.getOrDefault(killer.UserID, 0);
            playerScores.put(killer.UserID, score + 1);
        }
        
        boolean allZombiesDead = api.TSPlayer.All.Where(p -> p.ActiveNPCs.Any(npc -> npc.NPCType == NPCType.Zombie)).Count() == 0;
        if (allZombiesDead) {
            waveCount++;
            api.TSPlayer.All.SendInfoMessage("Wave " + waveCount + " incoming!");
            
            for (int i = 0; i < waveCount; i++) {
                int spawnX = random.nextInt(200) - 100 + api.TSPlayer.All.First().TileX;
                int spawnY = random.nextInt(50) - 25 + api.TSPlayer.All.First().TileY;
                api.TSPlayer.All.SpawnNPC(NPCType.Zombie, 0, spawnX, spawnY);
            }
        }
    }
    
    @EventHandler(priority = EventPriority.Normal)
    public void onPlayerDeath(PlayerDeathEvent event) {
        TSPlayer player = event.TSPlayer;
        if (player == null || !player.ActiveNPCs.Any(npc -> npc.NPCType == NPCType.Zombie)) {
            return;
        }
        
        player.SendErrorMessage("You were killed by a zombie! Better luck next time!");
        
        boolean allPlayersDead = api.TSPlayer.All.Where(p -> p.ActiveNPCs.Any(npc -> npc.NPCType == NPCType.Zombie)).Count() == 0;
        if (allPlayersDead) {
            isRunning = false;
            api.TSPlayer.All.SendInfoMessage("All players have been killed! Zombie minigame has ended!");
            playerScores.clear();
        }
    }
    
    @EventHandler(priority = EventPriority.Normal)
    public void onPlayerLeave(PlayerLeaveEvent event) {
        TSPlayer player = event.TSPlayer;
        if (player == null || !player.ActiveNPCs.Any(npc -> npc.NPCType == NPCType.Zombie)) {
            return;
        }
        
        player.SendErrorMessage("You left the game! You forfeit your score!");
        
        boolean allPlayersLeft = api.TSPlayer.All.Where(p -> p.ActiveNPCs.Any(npc -> npc.NPCType == NPCType.Zombie)).Count() == 0;
        if (allPlayersLeft) {
            isRunning = false;
            api.TSPlayer.All.SendInfoMessage("All players have left the game! Zombie minigame has ended!");
            playerScores.clear();
        } else {
            playerScores.remove(player.UserID);
        }
    }
}
