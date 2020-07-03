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
        public async Task TickWarCounter([HttpTrigger(AuthorizationLevel.Anonymous, "post", "/tickwarcounter")] HttpRequestMessage request,
            [DurableClient] IDurableEntityClient client)
        {
            var counterTick = await JsonSerializer.DeserializeAsync<WarCounterTick>(await request.Content.ReadAsStreamAsync());
            var warId = new WarId($"{counterTick.SourceUser}_{counterTick.TargetUser}");

            await client.SignalEntityAsync(new EntityId(nameof(WarCounter), warId), "Tick",
                counterTick.AmountTaken);
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
