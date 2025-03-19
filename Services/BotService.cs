using System;
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
