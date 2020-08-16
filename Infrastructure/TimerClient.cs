using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PointsBot.Core;
using PointsBot.Core.Configuration;

namespace PointsBot.Infrastructure
{
    public class TimerClient : ICosmosTimerClient
    {
        private readonly Func<string, Container> _containerFactory;
        private readonly TimeoutOptions _timeout;

        public TimerClient(Func<string, Container> containerFactory, IOptions<TimeoutOptions> timeout)
        {
            _containerFactory = containerFactory;
            _timeout = timeout.Value;
        }

        public async Task<ResponseMessage> GetPlayerTimeoutRecord(string source, string playerId)
        {
            var container = _containerFactory(_timeout.ContainerName);
            var existingItem = await container.ReadItemStreamAsync($"{source}_{playerId}", new PartitionKey(source));

            if (existingItem.StatusCode == HttpStatusCode.InternalServerError) throw new Exception($"Error occured when attempting to see if player ({playerId}) is timed out");

            return existingItem;
        }

        public Task CreatePlayerTimeoutRecord(string source, string playerId)
        {
            var container = _containerFactory(_timeout.ContainerName);
            return container.CreateItemAsync(new TimeoutRecord
                { source = source, PlayerId = playerId, ttl = _timeout.InSeconds });
        }

        private class TimeoutRecord
        {
            // ReSharper disable once MemberCanBePrivate.Local (Serialization)
            public string id => $"{source}_{PlayerId}";

            public string source { get; set; }

            [JsonPropertyName("playerId")]
            public string PlayerId { get; set; }

            public int ttl { get; set; }

            public string GetId() => id;
        }
    }
}