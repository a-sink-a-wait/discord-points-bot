using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Function.Command.Events;
using StackExchange.Redis;

namespace Function.Query.Redis
{
    public class RedisPointsEventStorage : IEventStorage<PointsEvent>
    {
        private readonly IConnectionMultiplexer _redis;

        private static string PlayerKey(string playerId) => $"points_{playerId}";

        public RedisPointsEventStorage(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<IEnumerable<PointsEvent>> GetEvents(string playerId)
        {
            var database = _redis.GetDatabase();

            var eventListLength = await database.ListLengthAsync(PlayerKey(playerId));
            var redisValueTasks = new List<Task<RedisValue>>();

            for (long ii = 0; ii <= eventListLength; ii++)
            {
                redisValueTasks.Add(database.ListGetByIndexAsync(PlayerKey(playerId), ii));
            }

            var redisValues = (await Task.WhenAll(redisValueTasks)).ToList();
            return redisValues.Select(value => JsonSerializer.Deserialize<PointsEvent>(Encoding.UTF8.GetBytes(value)));
        }
    }
}