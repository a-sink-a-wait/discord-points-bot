using System;
using System.Linq;
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

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            // Hook CommandExecuted to handle post-command-execution logic.
            _commands.CommandExecuted += CommandExecutedAsync;
            // Hook MessageReceived so we can process each message to see
            // if it qualifies as a command.
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            _commands.AddTypeReader<int>(new Int32TypeReader(), true);
            // Register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            if (_discord.GetUser(message.Author.Id) is SocketGuildUser guildUser)
            {
                var hasNobodyLikesMeRole = guildUser.Roles.Any(role => role.Name == "Nobody Likes Me");
                if (hasNobodyLikesMeRole)
                {
                    var nobodyLikesMeTasks = new[]
                    {
                        message.DeleteAsync(new RequestOptions {AuditLogReason = "Automated from PBot"}),
                        message.Author.SendMessageAsync("Go away.")
                    };

                    await Task.WhenAll(nobodyLikesMeTasks);

                    return;
                }
            }

            var argPos = 0;
            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
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
