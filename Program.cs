using BotDiscordCore.Core;
using BotDiscordCrypto.Services;
using Serilog;
using System.Text;

namespace BotDiscordCrypto
{
    internal class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var botDiscord = new BotDiscord();
            botDiscord.AddBotService<mBotService>();
            Log.Information($"Start ....");

            await botDiscord.ConfigureAsync();

            await botDiscord.StartAsync();
        }
    }
}
