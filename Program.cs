﻿using System;
using System.Threading.Tasks;
using DiscordBot.Services;

namespace DiscordBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            BotService bot = new BotService();
            await bot.StartAsync();

            await Task.Delay(-1);
        }
    }
}