using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Durable.Entity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Durable
{
    public class WarCounterEndpoint
    {
        [FunctionName("TickWarCounter")]
        public async Task TickWarCounter([DurableClient] IDurableEntityClient client,
            [HttpTrigger(AuthorizationLevel.Function, "post", "/warcounter/tick")] HttpRequestMessage request)
        {
            var counterTick = await JsonSerializer.DeserializeAsync<WarCounterTick>(await request.Content.ReadAsStreamAsync());
            var warId = new WarId($"{counterTick.SourceUser}_{counterTick.TargetUser}");

            await client.SignalEntityAsync<ICounter>(new EntityId(nameof(WarCounter), warId),
                counter => counter.Tick(counterTick.AmountTaken));
        }

        [FunctionName("GetWarCounter")]
        public async Task<int> GetWarCounter([DurableClient] IDurableEntityClient client,
            [HttpTrigger(AuthorizationLevel.Function, "get", "/warcounter/{warId}")]
            string warId)
        {
            var id = new WarId(warId);
            var counter = await client.ReadEntityStateAsync<WarCounter>(new EntityId(nameof(WarCounter), id));

            return !counter.EntityExists ? Int32.MinValue : counter.EntityState.PointsFromThreshold;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        // Request model
        private class WarCounterTick
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // Request model
            public string SourceUser { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // Request model
            public string TargetUser { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // Request model
            public int AmountTaken { get; set; }
        }
    }
}
