// ImageBuilderPlugin.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ImageBuilderPlugin
{
    [ApiVersion(2, 1)]
    public class ImageBuilderPlugin : TerrariaPlugin
    {
        // Path to where images are stored
        private static readonly string ImageFolderPath = Path.Combine(TShock.SavePath, "images");
        
        // Dictionary mapping colors to Terraria block types
        private static readonly Dictionary<Color, int> BlockColorMap = new Dictionary<Color, int>
        {
            { Color.FromArgb(255, 255, 255, 255), 166 }, // White - Snow Block
            { Color.FromArgb(255, 0, 0, 0), 10 },        // Black - Obsidian
            { Color.FromArgb(255, 153, 110, 72), 0 },    // Dirt color - Dirt Block
            { Color.FromArgb(255, 111, 83, 59), 0 },     // Dirt color - Dirt Block
            { Color.FromArgb(255, 140, 140, 140), 1 },   // Stone color - Stone Block
            { Color.FromArgb(255, 180, 110, 40), 2 },    // Wood color - Wood Platform (as wood block)
            { Color.FromArgb(255, 255, 255, 0), 183 },   // Yellow - Sunflower
            { Color.FromArgb(255, 0, 255, 0), 53 },      // Green - Grass Block
            { Color.FromArgb(255, 0, 0, 255), 19 },      // Blue - Water
            { Color.FromArgb(255, 255, 0, 0), 15 },      // Red - Stone Slab (as red block)
            { Color.FromArgb(255, 128, 0, 0), 159 },     // Brown - Acorn
            { Color.FromArgb(255, 255, 128, 0), 18 },    // Orange - Honey Block
            { Color.FromArgb(255, 255, 192, 203), 55 },  // Pink - Pink Ice Block
            { Color.FromArgb(255, 128, 128, 128), 1 },   // Gray - Stone Block
            { Color.FromArgb(255, 128, 0, 128), 160 }    // Purple - Purple Ice Block
        };

        public override string Name => "ImageBuilder";
        public override string Author => "TShock Plugin Developer";
        public override Version Version => new Version(1, 0, 0);
        public override string Description => "Builds pixel art from images using blocks";

        // Track active builds to prevent multiple builds per player
        private readonly Dictionary<int, bool> _activeBuilds = new Dictionary<int, bool>();

        public ImageBuilderPlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            // Create images folder if it doesn't exist
            Directory.CreateDirectory(ImageFolderPath);

            // Register the command with permission
            Commands.ChatCommands.Add(new Command("imagebuilder.use", BuildCommand, "imagebuild")
            {
                HelpText = "Builds pixel art from an image: /imagebuild <imageName> <width> <height>"
            });

            // Log initialization info
            TShock.Log.Info($"[ImageBuilder] Plugin initialized. Images should be placed in: {ImageFolderPath}");
        }

        /// <summary>
        /// Handles the /imagebuild command
        /// </summary>
        private void BuildCommand(CommandArgs args)
        {
            // Check permission
            if (!args.Player.HasPermission("imagebuilder.use"))
            {
                args.Player.SendErrorMessage("You don't have permission to use this command!");
                return;
            }

            // Validate arguments
            if (args.Parameters.Count != 3)
            {
                args.Player.SendInfoMessage("Usage: /imagebuild <imageName> <width> <height>");
                args.Player.SendInfoMessage("Example: /imagebuild logo 50 30");
                return;
            }

            // Parse dimensions
            if (!int.TryParse(args.Parameters[1], out int width) || width <= 0)
            {
                args.Player.SendErrorMessage("Width must be a positive number!");
                return;
            }
            if (!int.TryParse(args.Parameters[2], out int height) || height <= 0)
            {
                args.Player.SendErrorMessage("Height must be a positive number!");
                return;
            }

            // Check for existing build
            if (_activeBuilds.TryGetValue(args.Player.Index, out bool isActive) && isActive)
            {
                args.Player.SendErrorMessage("You're already building an image! Wait for it to finish.");
                return;
            }

            // Find the image file
            string fileName = args.Parameters[0];
            string[] extensions = { ".png", ".jpg", ".jpeg" };
            string imagePath = extensions
                .Select(ext => Path.Combine(ImageFolderPath, fileName + ext))
                .FirstOrDefault(File.Exists);

            if (string.IsNullOrEmpty(imagePath))
            {
                args.Player.SendErrorMessage($"Image not found! Check if {fileName}.png/.jpg exists in {ImageFolderPath}");
                return;
            }

            // Mark build as active
            _activeBuilds[args.Player.Index] = true;
            
            // Process in background to avoid server lag
            Task.Run(() => ProcessImage(args.Player, imagePath, width, height));
        }

        /// <summary>
        /// Processes the image and starts the build
        /// </summary>
        private void ProcessImage(TSPlayer player, string imagePath, int width, int height)
        {
            try
            {
                // Load and resize image
                using (var image = new Bitmap(imagePath))
                {
                    var resizedImage = ResizeImage(image, width, height);
                    var blockGrid = ConvertToBlocks(resizedImage);

                    // Start building in main thread (safe for WorldGen)
                    Main.QueueMainThreadAction(() => 
                        StartBuilding(player, blockGrid, player.TileX, player.TileY));
                }
            }
            catch (Exception ex)
            {
                player.SendErrorMessage($"Error processing image: {ex.Message}");
                _activeBuilds[player.Index] = false;
            }
        }

        /// <summary>
        /// Resizes image while maintaining aspect ratio, then crops to exact dimensions
        /// </summary>
        private Bitmap ResizeImage(Image image, int targetWidth, int targetHeight)
        {
            // Calculate proportional scaling
            float ratio = Math.Min(
                (float)targetWidth / image.Width,
                (float)targetHeight / image.Height
            );

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            // Create new image with target dimensions
            var resized = new Bitmap(targetWidth, targetHeight);
            using (var g = Graphics.FromImage(resized))
            {
                // Center the image and fill with transparent background
                int x = (targetWidth - newWidth) / 2;
                int y = (targetHeight - newHeight) / 2;
                g.Clear(Color.Transparent);
                g.DrawImage(image, x, y, newWidth, newHeight);
            }
            return resized;
        }

        /// <summary>
        /// Converts image to block grid using color mapping
        /// </summary>
        private int[,] ConvertToBlocks(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            var blockGrid = new int[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    
                    // Skip transparent pixels
                    if (pixel.A < 128)
                    {
                        blockGrid[y, x] = -1; // -1 = air (no block)
                        continue;
                    }

                    // Find closest matching block
                    blockGrid[y, x] = FindClosestBlock(pixel);
                }
            }
            return blockGrid;
        }

        /// <summary>
        /// Finds the closest block type for a given color
        /// </summary>
        private int FindClosestBlock(Color color)
        {
            double minDistance = double.MaxValue;
            int closestBlock = -1; // Default to air

            foreach (var entry in BlockColorMap)
            {
                double distance = ColorDistance(color, entry.Key);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestBlock = entry.Value;
                }
            }
            return closestBlock;
        }

        /// <summary>
        /// Calculates Euclidean distance between two colors
        /// </summary>
        private double ColorDistance(Color c1, Color c2)
        {
            int rDiff = c1.R - c2.R;
            int gDiff = c1.G - c2.G;
            int bDiff = c1.B - c2.B;
            return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        }

        /// <summary>
        /// Starts the building process in chunks to avoid lag
        /// </summary>
        private void StartBuilding(TSPlayer player, int[,] blockGrid, int startX, int startY)
        {
            int width = blockGrid.GetLength(1);
            int height = blockGrid.GetLength(0);
            int totalPixels = width * height;
            int processed = 0;
            int chunkSize = 50; // Process 50 pixels per frame

            void BuildChunk()
            {
                try
                {
                    // Build in small chunks to avoid lag
                    for (int i = 0; i < chunkSize; i++)
                    {
                        if (processed >= totalPixels)
                        {
                            // Finish building
                            player.SendSuccessMessage($"Image built successfully! ({width}x{height})");
                            _activeBuilds[player.Index] = false;
                            return;
                        }

                        int y = processed / width;
                        int x = processed % width;

                        int blockType = blockGrid[y, x];
                        if (blockType != -1) // Skip air
                        {
                            // Place block at relative position
                            int worldX = startX + x;
                            int worldY = startY + y;
                            
                            // Safety checks
                            if (WorldGen.InWorld(worldX, worldY, 1))
                            {
                                WorldGen.PlaceTile(worldX, worldY, blockType, true, true, -1, 0);
                            }
                        }

                        processed++;
                    }

                    // Update progress every 10%
                    int progress = (int)Math.Round((double)processed / totalPixels * 100);
                    if (progress % 10 == 0 && progress > 0)
                    {
                        player.SendInfoMessage($"Building... {progress}% complete");
                    }

                    // Schedule next chunk
                    if (processed < totalPixels)
                    {
                        TShock.Utils.ScheduleNextFrame(BuildChunk);
                    }
                    else
                    {
                        player.SendSuccessMessage($"Image built successfully! ({width}x{height})");
                        _activeBuilds[player.Index] = false;
                    }
                }
                catch (Exception ex)
                {
                    player.SendErrorMessage($"Build error: {ex.Message}");
                    _activeBuilds[player.Index] = false;
                }
            }

            // Start building process
            BuildChunk();
        }
    }
}                return;
            }

            // Select a random item from the reward pool
            var reward = _rewardItems[_random.Next(_rewardItems.Count)];
            int stackSize = _random.Next(reward.minStack, reward.maxStack + 1);

            // Give the item to the player
            target.GiveItem(reward.itemId, stackSize);

            // Notify both players
            target.SendSuccessMessage($"You received {stackSize} x {Item.GetItemName(reward.itemId)} as a reward!");
            args.Player.SendSuccessMessage($"You gave {target.Name} {stackSize} x {Item.GetItemName(reward.itemId)}.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // No hooks to deregister
            }
            base.Dispose(disposing);
        }
    }
}
