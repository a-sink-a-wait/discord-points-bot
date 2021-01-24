using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bot.Services;
using Discord;
using Discord.Commands;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Configuration;
using PointsBot.Infrastructure.Commands;
using PointsBot.Infrastructure.Models;

namespace Bot.Modules
{
    public class PointsModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandSender _sender;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PointsService _pointsService;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private static string Source(ulong guildId) => $"discord_{guildId}";

        public PointsModule(CommandSender sender, IHttpClientFactory httpClientFactory, IConfiguration configuration, PointsService pointsService)
        {
            _sender = sender;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _pointsService = pointsService;
        }

        private static bool PlayerTargetingSelf(IUser source, IUser target) => source.Username.Equals(target.Username, StringComparison.InvariantCultureIgnoreCase);

        private async Task TrySendCommand(IUser user, Func<IEnumerable<Task>> commandsToSend)
        {
            if (await _pointsService.IsPlayerTimedOut(Context.User.Username, Source(Context.Guild.Id)))
            {
                await Context.User.SendMessageAsync(
                    "You're doing that too much. You can only add or remove points once an hour. I should probably tell you that because TECHNICALLY I know exactly when you're going to timeout. However my creator has neglected to see value in such a thing until just this moment...huh.......weird.");
                return;
            }

            if (PlayerTargetingSelf(Context.User, user))
            {
                await Context.User.SendMessageAsync("You can't `give` or `take` from yourself. It's just a game...'");
                return;
            }

            await Task.WhenAll(commandsToSend());
        }

        private IEnumerable<Task> RemovePoints(IUser user, int amountOfPoints) => new[]
        {
            _sender.SendRemove(Context.User.Username, user.Username, amountOfPoints, Source(Context.Guild.Id)),
            Context.Channel.SendMessageAsync("Transaction complete.")
        };

        private IEnumerable<Task> AddPoints(IUser user, int amountOfPoints) => new[]
        {
            _sender.SendAdd(Context.User.Username, user.Username, amountOfPoints, Source(Context.Guild.Id)),
            Context.Channel.SendMessageAsync("Transaction complete.")
        };

        [Command("give", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task GiveWithNoAmount(IGuildUser user) => await Context.Channel.SendMessageAsync($"Must specify an amount to give. Try @PBot give {user.Mention} 420");

        [Command("give", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task GivePoints(IGuildUser user, string amountOfPoints) => await Context.Channel.SendMessageAsync($"`give` only accepts numbers. Maybe some day we give and take words...");

        [Command("give", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task GivePoints(IGuildUser user, int amountOfPoints) => await TrySendCommand(user, () => AddPoints(user, amountOfPoints));

        [Command("give", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task GivePoints(IGuildUser user, int amountOfPoints, [Remainder] string theRest) => await TrySendCommand(user, () => AddPoints(user, amountOfPoints));

        [Command("take", RunMode = RunMode.Async)]
        public async Task TakeWithNoAmount(IGuildUser user) => await Context.Channel.SendMessageAsync($"Must specify an amount to take. Try @PBot take {user.Mention} 69");

        [Command("take", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task TakePoints(IGuildUser user, string amountOfPoints) => await Context.Channel.SendMessageAsync($"`take` only accepts numbers. . Maybe some day we give and take words...");

        [Command("take", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task TakePoints(IGuildUser user, int amountOfPoints) => await TrySendCommand(user, () => RemovePoints(user, amountOfPoints));

        [Command("take", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task TakePoints(IGuildUser user, int amountOfPoints, [Remainder] string theRest) => await TrySendCommand(user, () => RemovePoints(user, amountOfPoints));

        [Command("bank", RunMode = RunMode.Async)]
        public async Task GetTotalForUser(IGuildUser user)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var playerPointsResult = await httpClient.GetAsync($"{_configuration["QueryBaseEndpoint"]}points/{Source(Context.Guild.Id)}/{user.Username}?code={_configuration["QueryKey"]}");
            var playerState =
                JsonSerializer.Deserialize<PlayerState>(await playerPointsResult.Content.ReadAsStringAsync(), JsonOptions);

            await Context.Channel.SendMessageAsync($"{playerState.TotalPoints}");
        }

        [Command("bank", RunMode = RunMode.Async)]
        public async Task GetTotalForUser()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var playerPointsResult = await httpClient.GetAsync($"{_configuration["QueryBaseEndpoint"]}points/{Source(Context.Guild.Id)}/{Context.User.Username}?code={_configuration["QueryKey"]}");
            var playerState =
                JsonSerializer.Deserialize<PlayerState>(await playerPointsResult.Content.ReadAsStringAsync(), JsonOptions);

            await Context.Channel.SendMessageAsync($"{playerState.TotalPoints}");
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task PrintHelp()
        {
            await Context.User.SendMessageAsync(
                $"You can mention me and tell me to `give` or `take` points from another player by tagging them. Then tell me the amount of points you want to give or take. Eg: @Pbot give {Context.User.Mention} 1000. \r\n\r\n " +
                $"If you want to see how many points you currently have try mentioning me and saying `bank`. \r\n " +
                $"If you want to see the total of another player go ahead and mention me like @PBot `bank` {@Context.User.Mention}. \r\n\r\n" +
                $"You can only give or take an amount of points from a player every hour! Make it count!! \r\n" +
                $"You cannot give or take negative points or more points than 1000.");
        }
    }
}
