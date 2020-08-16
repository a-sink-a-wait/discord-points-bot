using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Durable.Entity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using PointsBot.Infrastructure.Models;

namespace Durable
{
    public class WarCounterSubscriber
    {
        private const string TopicName = "extensions";
        private const string SubscriptionName = "WarCounter";

        [FunctionName("WarCounterSubscriber")]
        public Task TickWarCounter([DurableClient] IDurableEntityClient client,
            [ServiceBusTrigger(TopicName, SubscriptionName, Connection = "ExtensionsConnectionString")]string message)
        {
            var counterTick = JsonSerializer.Deserialize<WarCounterTick>(message);
            if (!WarId.TryParse($"{counterTick.SourceUser}_{counterTick.TargetUser}", out var warId))
                throw new InvalidDataException($"Invalid warId: {warId}");

            return client.SignalEntityAsync<ICounter>(new EntityId(nameof(WarCounter), warId),
                counter => counter.Tick(counterTick.AmountTaken));
        }
    }
}