using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands;

[SlashCommandGroup("embed", "Make and edit embeds")]
[SlashCommandPermissions(Permissions.ManageMessages)]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
public class EmbedCommands: ApplicationCommandModule
{
    private readonly ILogger<MessageCommands> _logger;

    public EmbedCommands(ILogger<MessageCommands> logger)
    {
        _logger = logger;
    }

    [SlashCommand("create", "Creates embed")]
    public async Task CreateEmbed(
        InteractionContext ctx
        // [Option("title", "Title")] string title,
        // [Option("description", "Description")] string description = "",
        // [Option("url", "Embed url")] string url = "",
        // [Option("author", "author")] bool author = false,
        // [Option("fields", "Number of fields")] long fieldsNum = 0,
        // [Option("footer", "has footer")] bool footer = false,
        // [Option("color", "HexColor, like #FA23BC")] string color = "",
        // [Option("imageUrl", "imageUrl")] string imageUrl = "",
        // [Option("thumbnail", "has thumbnail")] bool thumbnail = false
    )
    {
        // DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
        // {
        //     Title = title,
        //     Url = url,
        //     Color = color == "" ? new Optional<DiscordColor>() : new DiscordColor(color),
        //     ImageUrl = imageUrl,
        //     Description = description
        // };
        //
        // if (author || footer || thumbnail || fieldsNum > 0)
        // {
        //     //await ctx.DeferAsync(true);
        // }
        // if (author)
        // {
        //     var modalId = $"embedbuilder-author-{ctx.InteractionId}";
        //     // await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done"));
        //     await ctx.CreateResponseAsync(InteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
        //         .WithTitle("Author of embed")
        //         .WithCustomId(modalId)
        //         .AddComponents(new TextInputComponent("Name", "name", required: true))
        //     );
        //     var interactivity = ctx.Client.GetInteractivity();
        //     var modalAuthor = await interactivity.WaitForModalAsync(modalId);
        //     await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(modalAuthor.Result.Values["name"]));
        // }
        // await ctx.Channel.SendMessageAsync(embedBuilder.Build());
        // // await ctx.CreateResponseAsync(ctx.User.Mention);
        DiscordComponent[] createEmbedMessageComponents()
        {
            return new[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger,
                    "create-embed-cancel", "Cancel"),
                new DiscordButtonComponent(ButtonStyle.Primary, "create-embed-edit-title", "Edit title"),
                new DiscordButtonComponent(ButtonStyle.Success, "create-embed-send", "Send")
            };
        }
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder() { Title = "Title", Description = "Description" };
        DiscordInteractionResponseBuilder responseBuilder =
            new DiscordInteractionResponseBuilder()
                .AsEphemeral(true)
                .AddEmbed(embedBuilder.Build())
                .AddComponents(
                    //new DiscordButtonComponent(ButtonStyle.Danger, "cancel-embed", "Cancel")//,
                    //new DiscordActionRowComponent(new[]
                    //    {
                            createEmbedMessageComponents()
                    //    }
                    //)
                );
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
        var interactivity = ctx.Client.GetInteractivity();
        bool done = false;
        while (!done)
        {
            var result = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), TimeSpan.FromMinutes(5));
            if (result.TimedOut)
            {
                done = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out"));
            }
            else if (result.Result.Id.EndsWith("send"))
            {
                await ctx.Channel.SendMessageAsync(
                    new DiscordMessageBuilder().WithEmbed(result.Result.Message.Embeds[0]));
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Sent").AsEphemeral(true));
                done = true;
            } else if (result.Result.Id.EndsWith("cancel"))
            {
                done = true;
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Canceled").AsEphemeral(true));
            } else if (result.Result.Id.EndsWith("edit-title"))
            {
                var embed = result.Result.Message.Embeds[0];
                var modifiedEmbed = new DiscordEmbedBuilder(embed);
                var modal = new DiscordInteractionResponseBuilder().WithTitle("Edit title")
                    .WithCustomId("create-embed-edit-title-modal")
                    .AddComponents(new TextInputComponent("Title", "title", required: true));
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                var modalResult = await interactivity.WaitForModalAsync("create-embed-edit-title-modal");
                modifiedEmbed.WithTitle(modalResult.Result.Values["title"]);
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().AddEmbed(modifiedEmbed.Build())
                        .AddComponents(createEmbedMessageComponents())
                );
                await modalResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("title edited").AsEphemeral(true));
            }
        }
    }
}
