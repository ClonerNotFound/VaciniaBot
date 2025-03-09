using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace VaciniaBot.commands
{
    public class PrefixCommands : BaseCommandModule
    {
        [Command(name: "hello")]
        public async Task Hello(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(content: "Привет!");
        }
    }
}