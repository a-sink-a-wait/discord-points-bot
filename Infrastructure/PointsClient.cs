using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PointsBot.Core;
using PointsBot.Core.Configuration;

namespace PointsBot.Infrastructure
{
    public class PointsClient : ICosmosPointsClient
    {
        private readonly EventFeedOptions _eventFeedOptions;
        private readonly Func<string, Container> _containerFactory;

        public PointsClient(IOptions<EventFeedOptions> eventFeedOptions, Func<string, Container> containerFactory)
        {
            _containerFactory = containerFactory;
            _eventFeedOptions = eventFeedOptions.Value;
        }

        public async Task<ICollection<PointsEvent>> GetPointsRecord(string source, string playerId)
        {
            var container = _containerFactory(_eventFeedOptions.ContainerName);
            var existingItem = await container.ReadItemStreamAsync($"{source}_{playerId}", new PartitionKey(source));

            if (existingItem.StatusCode == HttpStatusCode.InternalServerError) throw new Exception($"Error occured when attempting to see get the points record (Player:{playerId})");
            if (existingItem.StatusCode == HttpStatusCode.NotFound) await CreatePointsRecord(container, source, playerId);

            var response = await container.ReadItemStreamAsync($"{source}_{playerId}", new PartitionKey(source));
            var cosmosItem = await JsonSerializer.DeserializeAsync<CosmosItem>(response.Content);

            return cosmosItem.Events;
        }

        private static async Task CreatePointsRecord(Container container, string source, string playerId)
        {
            var createdRecordResponse = await container.CreateItemAsync(new CosmosItem
            {
                id = $"{source}_{playerId}",
                source = source,
                Events = new List<PointsEvent>()
            });

            if (createdRecordResponse.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception($"Error occured trying to create new player {playerId}");
        }

        public Task UpdatePlayer(string source, string playerId, ICollection<PointsEvent> newEvents)
        {
            var container = _containerFactory(_eventFeedOptions.ContainerName);
            var updatedPlayer = new CosmosItem
            {
                id = $"{source}_{playerId}",
                source = source,
                Events = newEvents
            };
            return container.UpsertItemAsync(updatedPlayer);
        }

        private class CosmosItem
        {
            public string id { get; set; }

            public string source { get; set; }

            public ICollection<PointsEvent> Events { get; set; }
        }
    }
}