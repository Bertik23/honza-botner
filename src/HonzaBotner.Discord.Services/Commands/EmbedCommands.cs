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
        ){
    DiscordComponent[] createEmbedMessageComponents()
        {
            return new DiscordComponent[]
            {
                new DiscordSelectComponent("create-embed-select-action", "Choose, what you want to do",
                    new[]
                    {
                        new DiscordSelectComponentOption("Cancel", "cancel", "Cancel creating embed"),
                        new DiscordSelectComponentOption("Title", "edit-title", "Edit title"),
                        new DiscordSelectComponentOption("Description", "edit-description", "Edit description"),
                        new DiscordSelectComponentOption("Color","edit-color", "Edit color"),
                        new DiscordSelectComponentOption("Url", "edit-url", "Edit url"),
                        new DiscordSelectComponentOption("Image url", "edit-imageUrl", "Edit image url"),
                        new DiscordSelectComponentOption("Author", "edit-author", "Edit author"),
                        new DiscordSelectComponentOption("Footer", "edit-footer", "Edit footer"),
                        new DiscordSelectComponentOption("Timestamp", "edit-timestamp", "Edit timestamp"),
                        new DiscordSelectComponentOption("Send", "send", "Send embed")
                    }),
            };
        }
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder() { Title = "Title", Description = "Description", Color = new DiscordColor("000000")};
        DiscordInteractionResponseBuilder responseBuilder =
            new DiscordInteractionResponseBuilder()
                .AsEphemeral(true)
                .AddEmbed(embedBuilder.Build())
                .AddComponents(
                    createEmbedMessageComponents()
                );
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
        var interactivity = ctx.Client.GetInteractivity();
        bool done = false;
        while (!done)
        {
            var action = "";
            var result =
                await interactivity.WaitForSelectAsync(await ctx.GetOriginalResponseAsync(), "create-embed-select-action",TimeSpan.FromMinutes(5));
            if (result.TimedOut)
            {
                done = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out"));
                return;
            }
            if (result.Result.Id.EndsWith("select-action"))
            {
                action = result.Result.Values[0];
            }
            else
            {
                // This should never happen
                _logger.Log(LogLevel.Warning, "Got unacceptable response");
                return;
            }

            try
            {
                // SEND
                if (action.EndsWith("send"))
                {
                    await ctx.Channel.SendMessageAsync(
                        new DiscordMessageBuilder().WithEmbed(result.Result.Message.Embeds[0]));
                    await result.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Sent").AsEphemeral(true));
                    done = true;
                }

                // CANCEL
                else if (action.EndsWith("cancel"))
                {
                    done = true;
                    await result.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Canceled").AsEphemeral(true));
                }

                // EDIT TITLE
                else if (action.EndsWith("edit-title"))
                {
                    var embed = result.Result.Message.Embeds[0];
                    var modifiedEmbed = new DiscordEmbedBuilder(embed);
                    var modal = new DiscordInteractionResponseBuilder().WithTitle("Edit title")
                        .WithCustomId("create-embed-edit-title-modal")
                        .AddComponents(new TextInputComponent("Title", "title", required: true, value: embed.Title));
                    await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    var modalResult = await interactivity.WaitForModalAsync("create-embed-edit-title-modal");
                    modifiedEmbed.WithTitle(modalResult.Result.Values["title"]);
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(modifiedEmbed.Build())
                            .AddComponents(createEmbedMessageComponents())
                    );
                    await modalResult.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("title edited").AsEphemeral(true));
                }

                // EDIT DESCRIPTION
                else if (action.EndsWith("edit-description"))
                {
                    var embed = result.Result.Message.Embeds[0];
                    var modifiedEmbed = new DiscordEmbedBuilder(embed);
                    var modal = new DiscordInteractionResponseBuilder().WithTitle("Edit description")
                        .WithCustomId("create-embed-edit-description-modal")
                        .AddComponents(new TextInputComponent("Description", "description", required: true,
                            value: embed.Description));
                    await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    var modalResult = await interactivity.WaitForModalAsync("create-embed-edit-description-modal");
                    modifiedEmbed.WithDescription(modalResult.Result.Values["description"]);
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(modifiedEmbed.Build())
                            .AddComponents(createEmbedMessageComponents())
                    );
                    await modalResult.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("description edited").AsEphemeral(true));
                }

                // EDIT COLOR
                else if (action.EndsWith("edit-color"))
                {
                    var embed = result.Result.Message.Embeds[0];
                    var modifiedEmbed = new DiscordEmbedBuilder(embed);
                    var modal = new DiscordInteractionResponseBuilder().WithTitle("Edit color")
                        .WithCustomId("create-embed-edit-color-modal")
                        .AddComponents(new TextInputComponent("Color", "color", required: true));
                    await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    var modalResult = await interactivity.WaitForModalAsync("create-embed-edit-color-modal");
                    modifiedEmbed.WithColor(new DiscordColor(modalResult.Result.Values["color"]));
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(modifiedEmbed.Build())
                            .AddComponents(createEmbedMessageComponents())
                    );
                    await modalResult.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("color edited").AsEphemeral(true));
                }

                // EDIT TIMESTAMP
                else if (action.EndsWith("edit-timestamp"))
                {
                    var embed = result.Result.Message.Embeds[0];
                    var modifiedEmbed = new DiscordEmbedBuilder(embed);
                    var modal = new DiscordInteractionResponseBuilder().WithTitle("Edit timestamp")
                        .WithCustomId("create-embed-edit-timestamp-modal")
                        .AddComponents(new TextInputComponent("Timestamp", "timestamp", required: true));
                    await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    var modalResult = await interactivity.WaitForModalAsync("create-embed-edit-timestamp-modal");
                    modifiedEmbed.WithTimestamp(
                        DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(modalResult.Result.Values["timestamp"]) *
                                                                1000));
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(modifiedEmbed.Build())
                            .AddComponents(createEmbedMessageComponents())
                    );
                    await modalResult.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("timestamp edited").AsEphemeral(true));
                }

                // EDIT URL
                else if (action.EndsWith("edit-url"))
                {
                    var embed = result.Result.Message.Embeds[0];
                    var modifiedEmbed = new DiscordEmbedBuilder(embed);
                    var modal = new DiscordInteractionResponseBuilder().WithTitle("Edit url")
                        .WithCustomId("create-embed-edit-url-modal")
                        .AddComponents(new TextInputComponent("Url", "url", required: true));
                    await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    var modalResult = await interactivity.WaitForModalAsync("create-embed-edit-url-modal");
                    modifiedEmbed.WithUrl(modalResult.Result.Values["url"]);
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(modifiedEmbed.Build())
                            .AddComponents(createEmbedMessageComponents())
                    );
                    await modalResult.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("url edited").AsEphemeral(true));
                }

                // EDIT IMAGE URL
                else if (action.EndsWith("edit-imageUrl"))
                {
                    var embed = result.Result.Message.Embeds[0];
                    var modifiedEmbed = new DiscordEmbedBuilder(embed);
                    var modal = new DiscordInteractionResponseBuilder().WithTitle("Edit image url")
                        .WithCustomId("create-embed-edit-imageUrl-modal")
                        .AddComponents(new TextInputComponent("Url", "url", required: true));
                    await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    var modalResult = await interactivity.WaitForModalAsync("create-embed-edit-imageUrl-modal");
                    modifiedEmbed.WithImageUrl(modalResult.Result.Values["url"]);
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(modifiedEmbed.Build())
                            .AddComponents(createEmbedMessageComponents())
                    );
                    await modalResult.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("url edited").AsEphemeral(true));
                }

                // EDIT AUTHOR
                else if (action.EndsWith("edit-author"))
                {
                    var embed = result.Result.Message.Embeds[0];
                    var modifiedEmbed = new DiscordEmbedBuilder(embed);
                    var modal = new DiscordInteractionResponseBuilder().WithTitle("Edit author")
                        .WithCustomId("create-embed-edit-author-modal")
                        .AddComponents(new TextInputComponent("Name", "name", required: false))
                        .AddComponents(new TextInputComponent("Url", "url", required: false))
                        .AddComponents(new TextInputComponent("Icon url", "iconUrl", required: false)
                        );
                    await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    var modalResult = await interactivity.WaitForModalAsync("create-embed-edit-author-modal");
                    modifiedEmbed.WithAuthor(modalResult.Result.Values["name"], modalResult.Result.Values["url"],
                        modalResult.Result.Values["iconUrl"]);
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(modifiedEmbed.Build())
                            .AddComponents(createEmbedMessageComponents())
                    );
                    await modalResult.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("author edited").AsEphemeral(true));
                }

                // EDIT FOOTER
                else if (action.EndsWith("edit-footer"))
                {
                    var embed = result.Result.Message.Embeds[0];
                    var modifiedEmbed = new DiscordEmbedBuilder(embed);
                    var modal = new DiscordInteractionResponseBuilder().WithTitle("Edit footer")
                        .WithCustomId("create-embed-edit-footer-modal")
                        .AddComponents(new TextInputComponent("Text", "text", required: false))
                        .AddComponents(new TextInputComponent("Icon url", "iconUrl", required: false)
                        );
                    await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    var modalResult = await interactivity.WaitForModalAsync("create-embed-edit-footer-modal");
                    modifiedEmbed.WithFooter(modalResult.Result.Values["text"], modalResult.Result.Values["iconUrl"]);
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(modifiedEmbed.Build())
                            .AddComponents(createEmbedMessageComponents())
                    );
                    await modalResult.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("footer edited").AsEphemeral(true));
                }
            }
            catch (Exception e)
            {
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Invalid input").AsEphemeral(true));
            }
        }
    }
}
