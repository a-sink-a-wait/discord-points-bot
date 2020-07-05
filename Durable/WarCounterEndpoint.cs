using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Durable.Entity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PointsBot.Infrastructure.Models;

namespace Durable
{
    public class WarCounterEndpoint
    {
        [FunctionName("TickWarCounter")]
        public async Task<HttpResponseMessage> TickWarCounter([DurableClient] IDurableEntityClient client,
            [HttpTrigger(AuthorizationLevel.Function, "post", "/warcounter/tick")] HttpRequestMessage request)
        {
            var counterTick = await JsonSerializer.DeserializeAsync<WarCounterTick>(await request.Content.ReadAsStreamAsync());
            if (!WarId.TryParse($"{counterTick.SourceUser}_{counterTick.TargetUser}", out var warId))
                return new HttpResponseMessage(HttpStatusCode.BadRequest) {ReasonPhrase = $"Invalid warId: {warId}"};

            await client.SignalEntityAsync<ICounter>(new EntityId(nameof(WarCounter), warId),
                counter => counter.Tick(counterTick.AmountTaken));

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        [FunctionName("GetWarCounter")]
        public async Task<HttpResponseMessage> GetWarCounter([DurableClient] IDurableEntityClient client,
            [HttpTrigger(AuthorizationLevel.Function, "get", "/warcounter/{warId}")]
            string warId)
        {
            if (!WarId.TryParse(warId, out var id))
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = $"Invalid warId: {warId}" };

            var counter = await client.ReadEntityStateAsync<WarCounter>(new EntityId(nameof(WarCounter), id));

            return !counter.EntityExists ? new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "No war" } : 
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(counter.EntityState.PointsFromThreshold.ToString())
                };
        }

        [FunctionName("ResetWarCounter")]
        public async Task<HttpResponseMessage> ResetWarCounter([DurableClient] IDurableEntityClient client,
            [HttpTrigger(AuthorizationLevel.Function, "post", "/warcounter/{warId}")]
            string warId)
        {
            if (!WarId.TryParse(warId, out var id))
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = $"Invalid warId: {warId}" };

            await client.SignalEntityAsync<ICounter>(new EntityId(nameof(WarCounter), id),
                counter => counter.Reset());
            
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
