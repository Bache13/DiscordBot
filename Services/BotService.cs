using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBot.Services
{
    public class BotService
    {
        private DiscordSocketClient _client = null!;

        public async Task StartAsync()
        {
            // Create the client with the necessary Gateway Intents
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
                                 | GatewayIntents.GuildMessages
                                 | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);

            // Subscribe to log events for debugging
            _client.Log += LogAsync;

            // Replace with your actual bot token
            string token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN")
                ?? throw new InvalidOperationException("DISCORD_BOT_TOKEN environment variable is not set");

            // Log in and start the bot
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Attach a basic message handler
            _client.MessageReceived += HandleMessageAsync;
        }

        private async Task HandleMessageAsync(SocketMessage message)
        {
            string raiderToken = Environment.GetEnvironmentVariable("RAIDER_API_TOKEN")
                ?? throw new InvalidOperationException("RAIDER_API_TOKEN environment variable is not set");

            var raiderService = new RaiderService(raiderToken, "season-tww-2", "eu");
            var cutoff = await raiderService.GetCutOffAsync();

            if (message.Content.Equals("!cutoff", StringComparison.OrdinalIgnoreCase))
            {
                if (cutoff.HasValue)
                {
                    await message.Channel.SendMessageAsync($"**Cutoff:** `{cutoff}`");
                }
                else
                {
                    await message.Channel.SendMessageAsync("There was an issue retrieving the data.");
                }
            }

            var raiderHighestKeyService = new RaiderService(raiderToken, "season-tww-2", "world", "all", 0);
            var highestKey = await raiderHighestKeyService.GetHighestKeyCompletedAsync();

            if (message.Content.Equals("!highest", StringComparison.OrdinalIgnoreCase))
            {
                if (highestKey.DungeonName == "Unknown Dungeon" || highestKey.MythicLevel == 0)
                {
                    await message.Channel.SendMessageAsync("The highest key wasn't found.");
                }
                else
                {
                    await message.Channel.SendMessageAsync($"**Highest key:** `{highestKey}`");
                }
            }

            if (message.Content.Equals("!popular", StringComparison.OrdinalIgnoreCase))
            {
                var processor = new RaiderDataProcessor(raiderToken, "season-tww-2", "eu", "all", 5);

                Dictionary<string, int> specCounts = await processor.GetTopSpecsFromAllPagesAsync();
                string popularSpecs = processor.FormatMostPopularGroup(specCounts);

                await message.Channel.SendMessageAsync(popularSpecs);
            }

            if (message.Content.Equals("!flask", StringComparison.OrdinalIgnoreCase))
            {
                Dictionary<string, string> allFlasks = new()
                {
                    { "Flask of Tempered Swiftness", "Haste" },
                    { "Flask of Alchemical Chaos", "Random Secondary Stat" },
                    { "Flask of Tempered Mastery", "Mastery" },
                    { "Flask of Tempered Versatility", "Versatility" },
                    { "Flask of Tempered Aggression", "Critical Strike" },
                    { "Flask of Saving Grace", "Healing increase" }
                };

                var sb = new StringBuilder();
                sb.AppendLine("**All available flasks:**");
                foreach (var flask in allFlasks)
                {
                    sb.AppendLine($"`{flask.Key} - {flask.Value}`");
                }

                await message.Channel.SendMessageAsync(sb.ToString());
            }

            // Rust pop

            if (message.Content.Equals("!main", StringComparison.OrdinalIgnoreCase))
            {
                var serverId = 2569573;

                string battleMetricsApiKey = Environment.GetEnvironmentVariable("BATTLEMETRICS_API_KEY")
                    ?? throw new InvalidOperationException("BATTLEMETRICS_API_KEY environment variable is not valid.");

                var RustService = new RustService(battleMetricsApiKey);

                try
                {
                    int currentPlayers = await RustService.GetPlayerCountFromSteviousMain(serverId);
                    await message.Channel.SendMessageAsync($"**Player count on Stevious Main**: `{currentPlayers}`");
                }
                catch (Exception ex)
                {
                    await message.Channel.SendMessageAsync($"Error retreiving data: {ex.Message}");
                }
            }

            if (message.Content.Equals("!monday", StringComparison.OrdinalIgnoreCase))
            {
                var serverId = 3261388;

                string battleMetricsApiKey = Environment.GetEnvironmentVariable("BATTLEMETRICS_API_KEY")
                    ?? throw new InvalidOperationException("BATTLEMETRICS_API_KEY environment variable is not valid.");

                var RustService = new RustService(battleMetricsApiKey);

                try
                {
                    int currentPlayers = await RustService.GetPlayerCountFromSteviousMain(serverId);
                    await message.Channel.SendMessageAsync($"**Player count on Stevious Monday**: `{currentPlayers}`");
                }
                catch (Exception ex)
                {
                    await message.Channel.SendMessageAsync($"Error retreiving data: {ex.Message}");
                }
            }

            // Ignore messages from other bots
            if (message.Author.IsBot)
                return;

            // Respond to the !hello command
            if (message.Content.Equals("!hello", StringComparison.OrdinalIgnoreCase))
            {
                await message.Channel.SendMessageAsync("Hello there!");
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
