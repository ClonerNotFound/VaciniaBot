using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MySql.Data.MySqlClient;
using VaciniaBot.config;

namespace VaciniaBot.commands.slash
{
    public class PlayerCommands : ApplicationCommandModule
    {
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
                        Title = "Список игроков",
                        Description = playerList,
                        Color = DiscordColor.Green
                    };

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
