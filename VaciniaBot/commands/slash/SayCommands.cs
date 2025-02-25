using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;

namespace VaciniaBot.commands.slash
{
    public class SayCommands : ApplicationCommandModule
    {
        [SlashCommand("say", "Отправить сообщение от имени бота")]
        public async Task SayCommand(InteractionContext ctx, [Option("текст", "Текст для отправки")] string text)
        {
            await ctx.DeferAsync(ephemeral: true);

            var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            if (!member.Permissions.HasPermission(Permissions.Administrator))
            {
                await ctx.CreateResponseAsync("У вас нет прав на использование этой команды.", ephemeral: true);
                return;
            }

            Console.WriteLine($"Пользователь {ctx.User.Username} выполнил команду /say");
            await ctx.Channel.SendMessageAsync(text);
        }
    }
}
