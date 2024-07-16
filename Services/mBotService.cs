using BotDiscordCore.Core.Services.Interfaces;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace BotDiscordCrypto.Services
{
    public class mBotService : IBotService
    {
        private readonly InteractionService _command;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;

        public mBotService(InteractionService command, DiscordSocketClient client, IServiceProvider services)
        {
            _command = command;
            _client = client;
            _services = services;
        }

        public async Task ConnectedClientAsync()
        {
            Log.Information($"{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator} connected ...");
            await _command.RegisterCommandsGloballyAsync();

            Log.Information($"Register {_command.SlashCommands.Count} Slash commands completed ...");
            foreach (var slashCommand in _command.SlashCommands)
            {
                Log.Information($"Register slash command [{slashCommand.Name}] completed ...");
            }

            await _client.SetActivityAsync(new Game("Bot Crypto <2024>"));
        }

        public async Task DisconnectedClientAsync(Exception e)
        {
            Log.Warning($"{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator} disconnected ...", e);
        }

        public async Task ReadyClientAsync()
        {
            var gateway = _client.GetBotGatewayAsync();
            Log.Information($"{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator} ready ...");
        }

        public async Task JoinedGuildAsync(SocketGuild guild)
        {
            Log.Information($"{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator} joined new guild [{guild.Name}]");
        }

        public async Task LeftGuildAsync(SocketGuild guild)
        {
            Log.Information($"{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator} left guild [{guild.Name}]");
        }
    }
}
