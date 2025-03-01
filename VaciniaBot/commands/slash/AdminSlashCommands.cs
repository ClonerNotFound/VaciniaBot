using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using VaciniaBot.config;
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

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embedMessage).AddComponents(dropdown));
        }
        [SlashCommand("removepassword", "Удалить пароль у игрока")]
        public async Task RemovePasswordCommand(InteractionContext ctx, [Option("Никнейм", "Никнейм игрока")] string nickname)
        {
            await ctx.DeferAsync(ephemeral: true);

            var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            if (!member.Permissions.HasPermission(Permissions.Administrator))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("У вас нет прав на использование этой команды."));
                return;
            }

            var jsonReader = new JSONReader();
            await jsonReader.ReadJson();

            var connectionString = $"Server={jsonReader.MySQL.Server};Port={jsonReader.MySQL.Port};Database={jsonReader.MySQL.Database};User ID={jsonReader.MySQL.User};Password={jsonReader.MySQL.Password};";

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand($"UPDATE {jsonReader.MySQL.Table} SET password = NULL WHERE last_name = @nickname", connection);
                    command.Parameters.AddWithValue("@nickname", nickname);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Пароль для игрока {nickname} успешно удален."));
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Игрок с ником {nickname} не найден."));
                    }
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Произошла ошибка при удалении пароля: {ex.Message}"));
                Console.WriteLine($"Ошибка при удалении пароля: {ex.Message}");
            }
        }
    }
}
