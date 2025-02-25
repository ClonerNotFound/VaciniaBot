﻿using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VaciniaBot.commands;
using VaciniaBot.commands.slash;
using VaciniaBot.config;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Google.Apis.YouTube.v3.Data;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Threading;

namespace VaciniaBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        public static CommandsNextExtension Commands { get; set; }
        public static async Task Main()
        {
            var jsonReader = new JSONReader();
            await jsonReader.ReadJson();

            var discordConfig = new DiscordConfiguration()
            {
                Token = jsonReader.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);
            Client.Ready += ClientOnReady;
            Client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            Client.ModalSubmitted += Client_ModalSubmitted;

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { jsonReader.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };

            var slashCommandsConfig = Client.UseSlashCommands();
            slashCommandsConfig.RegisterCommands<BasicSlashCommands>();

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<TestCommands>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
        private static async Task Client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            var jsonReader = new JSONReader();
            await jsonReader.ReadJson();

            if (args.Interaction.Data.CustomId == "ticket_dropdown")
            {
                await ModalTicketWindow(sender, args);
            }

            if (args.Interaction.Data.CustomId == "accept_button" || args.Interaction.Data.CustomId == "reject_button" || args.Interaction.Data.CustomId == "invite_button")
            {
                await HandleWhitelistButtons(sender, args, jsonReader);
            }
            if (args.Interaction.Data.CustomId == "delete_channel_button")
            {
                await DeleteButtonDropdown(sender, args);
            }
        }
        private static async Task DeleteButtonDropdown(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            await args.Interaction.DeferAsync();

            await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Канал будет удален через 10 секунд."));

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(10000, cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await args.Interaction.Channel.DeleteAsync();
                    }
                }
                catch (TaskCanceledException)
                {
                    // ничего не делаем
                }
            });

            var cancelButton = new DiscordButtonComponent(ButtonStyle.Secondary, "cancel_delete_button", "Отменить удаление");

            var cancelMessage = new DiscordMessageBuilder()
                .WithContent("Удаление канала через 10 секунд. Нажмите кнопку, чтобы отменить.")
                .AddComponents(cancelButton);

            await args.Interaction.Channel.SendMessageAsync(cancelMessage);

            sender.ComponentInteractionCreated += async (s, e) =>
            {
                if (e.Interaction.Data.CustomId == "cancel_delete_button")
                {
                    cancellationTokenSource.Cancel();

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("Удаление канала отменено."));

                    await e.Interaction.Channel.DeleteMessageAsync(e.Message);
                }
            };
        }
        private static async Task ModalTicketWindow(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            var selectedValue = args.Interaction.Data.Values[0];

            switch (selectedValue)
            {
                case "WhitelistRequest":
                    var modal = new DiscordInteractionResponseBuilder()
                        .WithTitle("Заявка в WhiteList")
                        .WithCustomId("whitelist_modal")
                        .AddComponents(new TextInputComponent(label: "Ваш никнейм", customId: "nickname", placeholder: "Введите ваш никнейм", required: true))
                        .AddComponents(new TextInputComponent(label: "Ваше имя", customId: "name", placeholder: "Введите ваше имя", required: true))
                        .AddComponents(new TextInputComponent(label: "Ваш возраст", customId: "age", placeholder: "Введите ваш возраст", required: true))
                        .AddComponents(new TextInputComponent(label: "Причина заявки", customId: "reason", placeholder: "Почему вы хотите попасть в WhiteList?", required: true))
                        .AddComponents(new TextInputComponent(label: "Дополнительная информация", customId: "additional_info", placeholder: "Дополнительные сведения", required: false));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    break;

                case "ReportViolation":
                    var modalReport = new DiscordInteractionResponseBuilder()
                        .WithTitle("Сообщить о нарушении")
                        .WithCustomId("report_modal")
                        .AddComponents(new TextInputComponent(label: "Никнейм нарушителя", customId: "violator_nickname", placeholder: "Введите никнейм нарушителя", required: true))
                        .AddComponents(new TextInputComponent(label: "Описание нарушения", customId: "violation_description", placeholder: "Опишите нарушение", required: true))
                        .AddComponents(new TextInputComponent(label: "Дополнительная информация", customId: "additional_info", placeholder: "Дополнительные сведения", required: false));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modalReport);
                    break;
            }
        }
        private static async Task HandleWhitelistButtons(DiscordClient sender, ComponentInteractionCreateEventArgs args, JSONReader jsonReader)
        {
            await args.Interaction.DeferAsync();

            var embed = args.Message.Embeds.FirstOrDefault();
            var nicknameField = embed?.Fields.FirstOrDefault(f => f.Name == "Никнейм");
            var nickname = nicknameField?.Value;

            var guild = await sender.GetGuildAsync(args.Interaction.Guild.Id);

            var member = await guild.GetMemberAsync(args.Interaction.User.Id);

            switch (args.Interaction.Data.CustomId)
            {
                case "accept_button":
                    try
                    {
                        await member.SendMessageAsync("Ваша заявка на Whitelist была принята!");

                        await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                            .WithContent("Заявка принята! Пользователь уведомлен."));

                        var logChannel = await sender.GetChannelAsync(jsonReader.LogChannelId);
                        if (logChannel != null)
                        {
                            string message = $"Заявка от пользователя {nickname} была принята.";
                            await logChannel.SendMessageAsync(message);
                        }

                        var consoleChannel = await sender.GetChannelAsync(jsonReader.СonsoleChannel);
                        if (consoleChannel != null)
                        {
                            string message = $"/whitelist add {nickname}";
                            await logChannel.SendMessageAsync(message);
                        }

                        var cancelButton = new DiscordButtonComponent(ButtonStyle.Danger, "cancel_delete_button", "Отменить удаление");
                        var deleteMessage = new DiscordMessageBuilder()
                            .WithContent("Канал будет удален через 10 секунд. Нажмите кнопку, чтобы отменить удаление.")
                            .AddComponents(cancelButton);

                        var deleteConfirmationMessage = await args.Interaction.Channel.SendMessageAsync(deleteMessage);

                        var cancellationTokenSource = new CancellationTokenSource();
                        var cancellationToken = cancellationTokenSource.Token;

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(10000, cancellationToken);

                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await args.Interaction.Channel.DeleteAsync();
                                }
                            }
                            catch (TaskCanceledException)
                            {
                                // ничего не делаем
                            }
                        });

                        sender.ComponentInteractionCreated += async (s, e) =>
                        {
                            if (e.Interaction.Data.CustomId == "cancel_delete_button" && e.Message.Id == deleteConfirmationMessage.Id)
                            {
                                cancellationTokenSource.Cancel();

                                var deleteTicketButton = new DiscordButtonComponent(ButtonStyle.Danger, "manual_delete_button", "Удалить тикет");

                                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                    new DiscordInteractionResponseBuilder()
                                        .WithContent("Удаление канала отменено.")
                                        .AddComponents(deleteTicketButton));

                                await deleteConfirmationMessage.DeleteAsync();
                            }

                            if (e.Interaction.Data.CustomId == "manual_delete_button")
                            {
                                await e.Interaction.Channel.DeleteAsync();
                                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                    new DiscordInteractionResponseBuilder().WithContent("Канал удален вручную.").AsEphemeral(true));
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке кнопки 'Принять': {ex.Message}");
                        await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                            .WithContent("Произошла ошибка при обработке заявки."));
                    }
                    break;

                case "reject_button":
                    try
                    {
                        try
                        {
                            await member.SendMessageAsync("Ваша заявка на Whitelist была отклонена.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Не удалось отправить личное сообщение пользователю: {ex.Message}");
                        }

                        await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                            .WithContent("Заявка отклонена! Пользователь уведомлен."));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке кнопки 'Отклонить': {ex.Message}");
                        await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                            .WithContent("Произошла ошибка при обработке заявки."));
                    }
                    break;

                case "invite_button":
                    try
                    {
                        var channel = args.Interaction.Channel;

                        await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                            .WithContent("Игрок приглашен в чат!"));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке кнопки 'Пригласить': {ex.Message}");
                        await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                            .WithContent("Произошла ошибка при обработке приглашения."));
                    }
                    break;
            }
        }
        private static async Task Client_ModalSubmitted(DiscordClient sender, ModalSubmitEventArgs args)
        {
            if (args.Interaction.Data.CustomId == "whitelist_modal")
            {
                var nickname = args.Values["nickname"];
                var name = args.Values["name"];
                var age = args.Values["age"];
                var reason = args.Values["reason"];
                var additionalInfo = args.Values["additional_info"];

                var embed = new DiscordEmbedBuilder()
                {
                    Title = "Новая заявка в WhiteList",
                    Color = DiscordColor.Green
                };
                embed.AddField("Никнейм", nickname, inline: true);
                embed.AddField("Имя", name, inline: true);
                embed.AddField("Возраст", age, inline: true);
                embed.AddField("Причина заявки", reason, inline: false);
                if (!string.IsNullOrEmpty(additionalInfo))
                {
                    embed.AddField("Дополнительная информация", additionalInfo, inline: false);
                }

                var guild = await sender.GetGuildAsync(args.Interaction.Guild.Id);

                var category = guild.Channels.Values.FirstOrDefault(c => c.Name == "Заявки" && c.Type == ChannelType.Category);
                if (category == null)
                {
                    category = await guild.CreateChannelCategoryAsync("Заявки");
                }

                var channelName = $"whitelist-заявка-{nickname.Replace(" ", "-")}";
                var channel = await guild.CreateTextChannelAsync(channelName, parent: category);

                var acceptButton = new DiscordButtonComponent(ButtonStyle.Success, "accept_button", "Принять");
                var rejectButton = new DiscordButtonComponent(ButtonStyle.Danger, "reject_button", "Отклонить");
                var inviteButton = new DiscordButtonComponent(ButtonStyle.Primary, "invite_button", "Пригласить игрока в чат");

                var messageBuilder = new DiscordMessageBuilder()
                    .AddEmbed(embed)
                    .AddComponents(acceptButton, rejectButton, inviteButton);

                await channel.SendMessageAsync(messageBuilder);

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"Ваша заявка успешно отправлена! Канал для вашей заявки: {channel.Mention}").AsEphemeral(true));
            }
            else if (args.Interaction.Data.CustomId == "report_modal")
            {
                var violatorNickname = args.Values["violator_nickname"];
                var violationDescription = args.Values["violation_description"];
                var additionalInfo = args.Values["additional_info"];

                var guild = await sender.GetGuildAsync(args.Interaction.Guild.Id);

                var jsonReader = new JSONReader();
                await jsonReader.ReadJson();
                var adminRoles = jsonReader.AdminRoles;

                var overwrites = new List<DiscordOverwriteBuilder>();

                var everyoneRole = guild.EveryoneRole;
                overwrites.Add(new DiscordOverwriteBuilder(everyoneRole)
                    .Deny(Permissions.AccessChannels));

                foreach (var roleId in adminRoles)
                {
                    var role = guild.GetRole(roleId);
                    if (role != null)
                    {
                        overwrites.Add(new DiscordOverwriteBuilder(role)
                            .Allow(Permissions.AccessChannels | Permissions.SendMessages | Permissions.ReadMessageHistory));
                    }
                }

                var category = guild.Channels.Values.FirstOrDefault(c => c.Name == "Жалобы" && c.Type == ChannelType.Category);
                if (category == null)
                {
                    category = await guild.CreateChannelCategoryAsync("Жалобы");
                }

                var channelName = $"жалоба-{violatorNickname.ToLower().Replace(" ", "-")}";
                var channel = await guild.CreateTextChannelAsync(channelName, parent: category);

                var reportEmbed = new DiscordEmbedBuilder()
                {
                    Title = "Новая жалоба на нарушение",
                    Color = DiscordColor.Red
                };
                reportEmbed.AddField("Никнейм нарушителя", violatorNickname, inline: true);
                reportEmbed.AddField("Описание нарушения", violationDescription, inline: false);
                if (!string.IsNullOrEmpty(additionalInfo))
                {
                    reportEmbed.AddField("Дополнительная информация", additionalInfo, inline: false);
                }

                var deleteButton = new DiscordButtonComponent(ButtonStyle.Danger, "delete_channel_button", "Удалить");

                var messageBuilder = new DiscordMessageBuilder()
                    .AddEmbed(reportEmbed)
                    .AddComponents(deleteButton);

                await channel.SendMessageAsync(messageBuilder);

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Ваша жалоба успешно отправлена! Канал для жалобы: {channel.Mention}").AsEphemeral(true));
            }
        }
        private static Task ClientOnReady(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
