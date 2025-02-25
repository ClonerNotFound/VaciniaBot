using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using MySql.Data.MySqlClient;
using VaciniaBot.config;

namespace VaciniaBot.commands.slash
{
    public class PlayerCommands : ApplicationCommandModule
    {
        [SlashCommand("players", "Получить список игроков сервера")]
        public async Task PlayersCommand(InteractionContext ctx)
        {
            var guild = await ctx.Client.GetGuildAsync(ctx.Guild.Id, withCounts: true);

            await Task.Delay(2000);

            var jsonReader = new JSONReader();
            await jsonReader.ReadJson();

            string connectionString = $"Server={jsonReader.MySQL.Server};Port={jsonReader.MySQL.Port};Database={jsonReader.MySQL.Database};User ID={jsonReader.MySQL.User};Password={jsonReader.MySQL.Password};";

            List<string> players = new List<string>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = $"SELECT {jsonReader.MySQL.Column} FROM {jsonReader.MySQL.Table}";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                players.Add(reader.GetString(jsonReader.MySQL.Column));
                            }
                        }
                    }
                }

                if (players.Count > 0)
                {
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = "Список игроков",
                        Description = string.Join("\n", players),
                        Color = DiscordColor.Green
                    };

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Список игроков пуст.").AsEphemeral(true));
                }
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Ошибка при получении списка игроков: {ex.Message}").AsEphemeral(true));
            }
        }
    }
}
