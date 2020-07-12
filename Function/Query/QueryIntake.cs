using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Function.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using PointsBot.Core;
using PointsBot.Infrastructure.Models;

namespace Function.Query
{
    public class QueryIntake
    {
        private readonly IGameTimer _gameTimer;

        public QueryIntake(IGameTimer gameTimer)
        {
            _gameTimer = gameTimer;
        }

        [FunctionName("GetPlayerPoints")]
        [StorageAccount("PointsReadModelConnectionString")]
        public async Task<PlayerState> GetPlayerPoints(
            [Table("points")] CloudTable pointsTable,
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "points/{source}/{playerId}")]
            HttpRequest _, string playerId, string source)
        {
            var queryAction = TableOperation.Retrieve<PlayerPoints>(source, playerId);
            var queryResult = await pointsTable.ExecuteAsync(queryAction);

            if (queryResult.HttpStatusCode == 404) return new PlayerState();
            if (queryResult.HttpStatusCode >= 500) throw new WebException($"Error when querying for player ({playerId}): {queryResult.Result}");

            var playerPoints = (PlayerPoints) queryResult.Result;
            return new PlayerState
            {
                PlayerId = playerPoints.PlayerName,
                TotalPoints = playerPoints.TotalPoints
            };
        }

        [FunctionName("GetTimeout")]
        public Task<bool> GetPlayerTimeout(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "timeout/{source}/{playerId}")]
            HttpRequest _, string playerId, string source)
        {
            return _gameTimer.HasTimeout(playerId, source);
        }
    }
}