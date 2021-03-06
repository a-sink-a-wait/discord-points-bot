using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        private const ulong PotatoId = 309042360246075403;
        private const string NobodyLikesMeRoleName = "Nobody Likes Me";

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            _commands.AddTypeReader<int>(new Int32TypeReader(), true);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            if (await NobodyLikesUser(message)) return;

            var argPos = 0;
            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        private async Task<bool> NobodyLikesUser(SocketMessage message)
        {
            var guild = _discord.GetGuild(PotatoId);
            var guildUser = guild.GetUser(message.Author.Id);
            if (guildUser == null || !guildUser.HasRole(NobodyLikesMeRoleName)) return false;

            var nobodyLikesMeTasks = new[]
            {
                message.DeleteAsync(new RequestOptions { AuditLogReason = "Automated from PBot" }),
                message.Author.SendMessageAsync("Go away.")
            };

            await Task.WhenAll(nobodyLikesMeTasks);
            return true;
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified || result.IsSuccess)
                return;

            if (HtmlInQueryResponse(result))
            {
                await context.Channel.SendMessageAsync($"It seems like my creator has forgotten to turn me on...");
                return;
            }

            // the command failed, let's notify the user that something happened.
            await context.Channel.SendMessageAsync($"error: {result}");
        }

        private static bool HtmlInQueryResponse(IResult result)
        {
            return result.Error == CommandError.Exception &&
                result.ErrorReason.Contains(
                    "'<' is an invalid start of a value",
                    StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
