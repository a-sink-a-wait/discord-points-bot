using System;
using System.Text.Json;
using System.Threading.Tasks;
using Function.Commands;
using Function.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using PointsBot.Core;
using PointsBot.Infrastructure;

namespace Function
{
    public class Startup : FunctionsStartup
    {
        private static readonly CosmosClientOptions CosmosClientOptions = new CosmosClientOptions
        {
            ApplicationName = "PBot"
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(
                provider => new Func<JsonDocument, PointsEvent>(
                    element => new EventFactory().Create(element)
                )
            );
            var configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

            builder.Services.AddSingleton<Func<int>>(() => Int32.Parse(configuration["MaxPointsPerAddOrSubtract"]));
            builder.Services.AddSingleton(new CosmosClient(configuration["CosmosConnectionString"],
                CosmosClientOptions));
            builder.Services.AddSingleton<Func<string, Container>>(serviceProvider =>
            {
                var client = serviceProvider.GetService<CosmosClient>();
                return containerName =>
                {
                    var database = client.GetDatabase(configuration["CosmosDatabaseName"]);
                    return database.GetContainer(containerName);
                };
            });

            builder.Services.AddSingleton<IEventWriter<PointsEvent>, CosmosEventWriter>();
            builder.Services.AddSingleton<IGameTimer, CosmosTimer>();
            builder.Services.AddSingleton<ICosmosTimerClient, TimerClient>();
            builder.Services.AddSingleton<ICosmosPointsClient, PointsClient>();

            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<PlayerCache>();

            builder.Services.AddOptions<TimeoutOptions>().Configure<IConfiguration>(
                (settings, config) => config.GetSection("Timeout").Bind(settings));

            builder.Services.AddOptions<EventFeedOptions>()
                .Configure<IConfiguration>((settings, config) => config.GetSection("EventFeed").Bind(settings));

            builder.Services.AddSingleton(
                CloudStorageAccount.Parse(configuration["PointsReadModelConnectionString"]));

            builder.Services.AddSingleton<Func<string, CloudTable>>(serviceProvider =>
            {
                var cloudStorageAccount = serviceProvider.GetRequiredService<CloudStorageAccount>();
                var tableClient = cloudStorageAccount.CreateCloudTableClient();

                return tableName => tableClient.GetTableReference(tableName);
            });

            builder.Services.AddSingleton<PointsStorage>();
            builder.Services.AddSingleton<PlayerStorage>();
        }
    }
}
