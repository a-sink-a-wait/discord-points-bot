using System.Threading.Tasks;
using PointsBot.Core;

namespace PointsBot.Infrastructure
{
    public class CosmosEventWriter : IEventWriter<PointsEvent>
    {
        private readonly ICosmosPointsClient _pointsClient;
        public CosmosEventWriter(ICosmosPointsClient pointsClient) { _pointsClient = pointsClient; }

        public async Task PushEvents(PointsEvent pointsEvent)
        {
            var playerPointsEvents =
                await _pointsClient.GetPointsRecord(pointsEvent.Source, pointsEvent.TargetPlayerId);

            playerPointsEvents.Add(pointsEvent);
            await _pointsClient.UpdatePlayer(pointsEvent.Source, pointsEvent.TargetPlayerId, playerPointsEvents);
        }
    }
}