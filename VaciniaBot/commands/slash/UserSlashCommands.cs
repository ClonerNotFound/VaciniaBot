using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using MySql.Data.MySqlClient;
using VaciniaBot.config;

namespace VaciniaBot.commands.slash
{
    public class UserSlashCommands : ApplicationCommandModule
    {
        [SlashCommand("say", "Отправить сообщение от имени бота")]
        public async Task SayCommand(InteractionContext ctx, [Option("текст", "Текст для отправки")] string text)
        {
            await ctx.DeferAsync(ephemeral: true);

            var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            if (!member.Permissions.HasPermission(Permissions.Administrator))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("У вас нет прав на использование этой команды."));
                return;
            }

            await ctx.Channel.SendMessageAsync(text);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Сообщение отправлено!"));

            Console.WriteLine($"Пользователь {ctx.User.Username} выполнил команду /say");
        }

        [SlashCommand("clear", "Удалить указанное количество сообщений в канале")]
        public async Task ClearCommand(InteractionContext ctx, [Option("количество", "Количество сообщений для удаления")] long amount)
        {
            await ctx.DeferAsync(ephemeral: true);

            var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            if (!member.Permissions.HasPermission(Permissions.Administrator))
            {
                await ctx.CreateResponseAsync("У вас нет прав на использование этой команды.", ephemeral: true);
                return;
            }

            try
            {
                if (amount <= 0 || amount > 1000)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Количество сообщений должно быть от 1 до 100."));
                    return;
                }

                var messages = await ctx.Channel.GetMessagesAsync((int)amount);
                await ctx.Channel.DeleteMessagesAsync(messages);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Удалено {amount} сообщений."));
                Console.WriteLine($"Пользователь {ctx.User.Username} удалил {amount} сообщений в канале {ctx.Channel.Name}");
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Произошла ошибка при удалении сообщений: {ex.Message}"));
                Console.WriteLine($"Ошибка при удалении сообщений: {ex.Message}");
            }
        }

        [SlashCommand("players", "Показать полный список игроков")]
        public async Task PlayersCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            Console.WriteLine($"Пользователь {ctx.User.Username} выполнил команду /say");

            var jsonReader = new JSONReader();
            await jsonReader.ReadJson();

            var connectionString = $"Server={jsonReader.MySQL.Server};Port={jsonReader.MySQL.Port};Database={jsonReader.MySQL.Database};User ID={jsonReader.MySQL.User};Password={jsonReader.MySQL.Password};";

            var players = new List<string>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand($"SELECT {jsonReader.MySQL.Column} FROM {jsonReader.MySQL.Table}", connection);
                    Console.WriteLine(command);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            players.Add(reader.GetString(0));
                        }
                    }
                }

                if (players.Count > 0)
                {
                    var playerList = string.Join("\n", players);
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = "Игроки Vacinia",
                        Description = playerList,
                        Color = DiscordColor.Green,
                    };
                    embed.WithThumbnail("https://i.imgur.com/BOSOs8H.png");

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Список игроков пуст."));
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Произошла ошибка при получении списка игроков: {ex.Message}"));
            }
        }
    }
}