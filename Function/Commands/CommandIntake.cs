using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using PointsBot.Core;

namespace Function.Commands
{
    public class CommandIntake
    {
        private readonly Func<JsonDocument, PointsEvent> _eventsFactory;
        private readonly IGameTimer _gameTimer;
        private readonly IEventWriter<PointsEvent> _pointsEventWriter;
        private readonly Func<int> _maxPointsPerAction;
        private readonly PlayerCache _playerCache;
        private readonly PlayerStorage _playerStorage;

        public CommandIntake(Func<JsonDocument, PointsEvent> eventsFactory, 
            Func<int> maxPointsPerAction, IGameTimer gameTimer, IEventWriter<PointsEvent> pointsEventWriter, PlayerCache playerCache, PlayerStorage playerStorage)
        {
            _eventsFactory = eventsFactory;
            _maxPointsPerAction = maxPointsPerAction;
            _gameTimer = gameTimer;
            _pointsEventWriter = pointsEventWriter;
            _playerCache = playerCache;
            _playerStorage = playerStorage;
        }
        
        [FunctionName("CommandIntake")]
        public Task InterpretCommand([ServiceBusTrigger("commands", Connection = "PointsBotQueueConnection")]string commandPayload)
        {
            var command = JsonDocument.Parse(commandPayload);

            var pointsEvent = _eventsFactory(command);
            if (pointsEvent == null) return Task.CompletedTask;
            if (pointsEvent.Amount <= 0 || pointsEvent.Amount > _maxPointsPerAction()) return Task.CompletedTask;

            var updateTasks = new[]
            {
                _pointsEventWriter.PushEvents(pointsEvent),
                _gameTimer.Timeout(pointsEvent.OriginPlayerId, pointsEvent.Source)
            };

            return Task.WhenAll(updateTasks);
        }

        [FunctionName("ChangeFeedProcessor")]
        public async Task ProcessChangeFeed(
            [CosmosDBTrigger("%CosmosDatabaseName%", "%EventFeed:ContainerName%", CreateLeaseCollectionIfNotExists = true, ConnectionStringSetting = "CosmosConnectionString")]
            IReadOnlyList<Document> changes)
        {
            if (changes.Count <= 0) return;

            var updatedPlayers = new List<PlayerPoints>();
            foreach (var change in changes)
            {
                var playerId = ParseId(change.Id);
                var playerPoints = await _playerCache.GetPlayer(playerId);

                var pointsEvents = change.GetPropertyValue<IEnumerable<PointsEvent>>("Events").ToList();
                if (pointsEvents.Count == playerPoints.LastEventIndex) continue;

                var eventIndex = playerPoints.LastEventIndex + 1;
                do
                {
                    var pointsEvent = pointsEvents[eventIndex];

                    if (pointsEvent.Action == "add") playerPoints.TotalPoints += pointsEvent.Amount;
                    else if (pointsEvent.Action == "remove") playerPoints.TotalPoints -= pointsEvent.Amount;

                    eventIndex++;

                } while (eventIndex < pointsEvents.Count);
                playerPoints.LastEventIndex = eventIndex - 1;

                await _playerStorage.UpdatePlayer(playerPoints);
                updatedPlayers.Add(playerPoints);
            }

            _playerCache.UpdatePlayer(updatedPlayers);
        }

        private static string ParseId(string documentId)
        {
            var splitId = documentId.Split('_');

            if (splitId.Length != 3) throw new Exception("Document Id is malformed.");
            var source = $"{splitId[0]}_{splitId[1]}";
            var targetPlayerId = splitId[2];

            return $"{source}_{targetPlayerId}";
        }
    }
}
