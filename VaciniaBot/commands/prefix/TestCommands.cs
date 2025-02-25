using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace VaciniaBot.commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command(name: "hello")]
        public async Task Hello(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync( content: "Привет!" );
        }

        [Command(name: "random")]
        public async Task Random(CommandContext ctx, int min, int max)
        {
            var randomValue = new System.Random().Next(min, max);
            await ctx.Channel.SendMessageAsync(content: ctx.User.Username + ", " + randomValue);
        }
        [Command(name: "embed")]
        public async Task Embed(CommandContext ctx)
        {
            var message = new DiscordEmbedBuilder()
            {
                Title = "Embed",
                Description = $"Данный embed создан по запросу {ctx.User.Username}",
                Color = DiscordColor.Azure
            };

            await ctx.Channel.SendMessageAsync(embed: message);
        }
    }
}
