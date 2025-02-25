using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;

namespace VaciniaBot.commands.slash
{
    public class AdminSlashCommands : ApplicationCommandModule
    {
        [SlashCommand(name: "ticket", description: "Отправка сообщение с выпадающим меню в канал для создания тикетов")]
        public async Task TestSlashCommand(InteractionContext ctx)
        {
            var options = new List<DiscordSelectComponentOption>
        {
            new DiscordSelectComponentOption("Отправить заявку в WhiteList", "WhitelistRequest", "Выберите, чтобы отправить заявку в WhiteList"),
            new DiscordSelectComponentOption("Сообщить о нарушении", "ReportViolation", "Выберите, чтобы сообщить о нарушении")
        };

            var dropdown = new DiscordSelectComponent("ticket_dropdown", "Выберите действие", options);

            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = "Меню тикетов",
                Description = "Выбери интересующие тебя действие из выпадающего меню:",
                Color = DiscordColor.Azure
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embedMessage).AddComponents(dropdown).AsEphemeral(true));
        }
    }
}
