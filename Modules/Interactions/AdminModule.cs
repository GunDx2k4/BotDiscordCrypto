using BotDiscordCore.Core.Attribute;
using BotDiscordCore.Core.Base;
using Discord;
using Discord.Interactions;

namespace BotDiscordCrypto.Modules.Interactions
{
    [BotPermission(GuildPermission.Administrator)]
    [UserPermission(GuildPermission.Administrator)]
    public class AdminModule : BaseInteractionModule
    {
        [SlashCommand("ping", "Test Bot")]
        public async Task HandlePingCommand()
        {
            await RespondAsync($"Pong!");
        }
    }
}
