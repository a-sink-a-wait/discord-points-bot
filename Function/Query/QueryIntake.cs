using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Function.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using PointsBot.Infrastructure.Models;

namespace Function.Query
{
    public class QueryIntake
    {
        private readonly GameState _gameState;
        private readonly IGameTimer _gameTimer;

        public QueryIntake(GameState gameState, IGameTimer gameTimer)
        {
            _gameState = gameState;
            _gameTimer = gameTimer;
        }

        [FunctionName("GetPlayerPoints")]
        [StorageAccount("PointsReadModelConnectionString")]
        public async Task<PlayerState> GetPlayerPoints(
            [Table("points")] CloudTable pointsTable,
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "points/{source}/{playerId}")]
            HttpRequest request, string playerId, string source)
        {
            var queryAction = TableOperation.Retrieve<PlayerPoints>(source, playerId);
            var queryResult = await pointsTable.ExecuteAsync(queryAction);

            if (queryResult.HttpStatusCode == 404) return PlayerState.Unknown;
            if (queryResult.HttpStatusCode >= 500) throw new WebException($"Error when querying for player ({playerId}): {queryResult.Result}");

            var playerPoints = (PlayerPoints) queryResult.Result;
            return new PlayerState(playerPoints.PlayerName, playerPoints.TotalPoints);
        }

        [FunctionName("GetTimeout")]
        public async Task<object> GetPlayerTimeout(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "timeout/{playerId}")]
            HttpRequest request, string playerId)
        {
            return _gameTimer.HasTimeout(playerId);
        }
    }
}