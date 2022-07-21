using System;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord;

public class DiscordWrapper
{
    public DiscordClient Client { get; }
    public InteractivityExtension Interactivity { get; }
    public SlashCommandsExtension SlashCommands { get; }

    public DiscordWrapper(IOptions<DiscordConfig> options, IServiceProvider services, ILoggerFactory loggerFactory)
    {
        DiscordConfig optionsConfig = options.Value;
        DiscordConfiguration config = new()
        {
            LoggerFactory = loggerFactory,
            Token = optionsConfig.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        };

        Client = new DiscordClient(config);

        InteractivityConfiguration iConfig = new()
        {
            Timeout = TimeSpan.FromMinutes(2), AckPaginationButtons = true
        };
        Interactivity = Client.UseInteractivity(iConfig);

        SlashCommandsConfiguration sConfig = new()
        {
            Services = services
        };
        SlashCommands = Client.UseSlashCommands(sConfig);

        Client.Logger.LogInformation("Starting with secret: {Token}", options.Value.Token);
    }
}
