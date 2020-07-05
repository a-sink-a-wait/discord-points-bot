using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Durable.Entity
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WarCounter : ICounter
    {
        [JsonProperty("sourcePlayer")]
        public string SourceUser { get; set; }

        [JsonProperty("targetUser")]
        public string TargetUser { get; set; }

        [JsonProperty("pointsTaken")]
        public int TotalPointsTaken { get; set; }

        private const int WarThreshold = 2000;

        public void Tick(int amount) => TotalPointsTaken += amount;

        public void Reset() => TotalPointsTaken = 0;

        public int PointsFromThreshold => WarThreshold - TotalPointsTaken;

        [FunctionName(nameof(WarCounter))]
        public static Task Run([EntityTrigger] IDurableEntityContext context) =>
            context.DispatchAsync<WarCounter>();
    }

    public interface ICounter
    {
        void Tick(int amount);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class War
    {
        public List<string> AlliedUsers { get; set; } = new List<string>();

        public List<string> EnemyUsers { get; set; } = new List<string>();

        public static Task Run([EntityTrigger] IDurableEntityContext context) => Task.CompletedTask;

        public void Start(string sourceUser, string targetUser)
        {
            AlliedUsers.Add(sourceUser);
            EnemyUsers.Add(targetUser);
        }

        public List<string> AddAlly(string user)
        {
            AlliedUsers.Add(user);
            return AlliedUsers;
        }

        public List<string> AddEnemy(string user)
        {
            EnemyUsers.Add(user);
            return EnemyUsers;
        }
    }
}
