using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PointsBot.Infrastructure.Commands;

namespace Bot
{
    class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .UseEnvironment("development")
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder
                        .AddEnvironmentVariables()
                        .AddJsonFile("./local.appsettings.json");
                })
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices();
                    b.AddAzureStorage();
                    b.Services.AddSingleton<DiscordSocketClient>()
                        .AddSingleton<CommandService>()
                        .AddSingleton<CommandHandlingService>()
                        .AddSingleton<HttpClient>()
                        .AddSingleton<IQueueClient>(provider =>
                            new QueueClient(provider.GetService<IConfiguration>()["ServiceBusConnectionString"], "commands"))
                        .AddSingleton<CommandSender>()
                        .AddSingleton<ITopicClient>(provider => 
                            new TopicClient(provider.GetService<IConfiguration>()["ServiceBusConnectionString"], "extensions"))
                        .AddHttpClient()
                        .AddSingleton<PointsService>();
                })
                .Build();

            var cancellationTokenSource = new CancellationTokenSource();
            using (host.StartAsync(cancellationTokenSource.Token))
            {
                var services = host.Services;
                var configuration = host.Services.GetRequiredService<IConfiguration>();
                var client = services.GetRequiredService<DiscordSocketClient>();

                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, configuration["token"]);
                await client.StartAsync();

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await Task.Delay(-1, cancellationTokenSource.Token);
            }
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }
    }
}
