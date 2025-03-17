using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Rest;

namespace MyMplusBot
{
    public class BotService
    {
        private DiscordSocketClient _client = null!;

        public async Task StartAsync()
        {

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
                                 | GatewayIntents.GuildMessages
                                 | GatewayIntents.MessageContent
            };

            // _client = new DiscordSocketClient(new DiscordSocketConfig
            // {
            //     GatewayIntents = GatewayIntents.All
            // });

            _client = new DiscordSocketClient(config);


            _client.Log += LogAsync;


            string token = "MTM1MTE1NjkyOTk5MjMzMTM1OA.GbO_uY.3gTTW_LjS2zXGmDnEPk30_wAPxzEUSe6phXc7Q";


            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Ready += Client_Ready;

            _client.InteractionCreated += OnInteractionCreated;

            await Task.Delay(-1);


            // _client.MessageReceived += HandleMessageAsync;
        }


        private async Task Client_Ready()
        {
            var helloCommand = new SlashCommandBuilder()
                .WithName("hello")
                .WithDescription("Bot says hello!")
                .Build();

            var byeCommand = new SlashCommandBuilder()
                .WithName("good bye")
                .WithDescription("Bot says good bye!")
                .Build();

            var commands = new ApplicationCommandProperties[]
            {
                helloCommand, byeCommand
            };

            ulong guildId = 1351153720456642580;
            var guild = _client.GetGuild(guildId);

            await guild.CreateApplicationCommandAsync(helloCommand);
        }

        private async Task OnInteractionCreated(SocketInteraction interaction)
        {
            if (interaction is SocketSlashCommand slashCommand)
            {
                switch (slashCommand.CommandName)
                {
                    case "hello":
                        await slashCommand.RespondAsync("Hello from slash command!");
                        break;
                }
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
