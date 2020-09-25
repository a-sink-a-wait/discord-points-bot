using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Bot
{
    public class Int32TypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Int32.TryParse(input, out var result)) return Task.FromResult(TypeReaderResult.FromSuccess(result));

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                "I don't know how to read that number, look how big it is. Definitely larger than 32 bits..."));
        }
    }
}