using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using PointsBot.Infrastructure.Models;

namespace PointsBot.Infrastructure.Commands
{
    public class CommandSender
    {
        private readonly IQueueClient _client;
        private readonly ITopicClient _topicClient;

        public CommandSender(IQueueClient client, ITopicClient topicClient)
        {
            _client = client;
            _topicClient = topicClient;
        }

        public Task SendAdd(string originPlayerId, string targetPlayerId, int amountOfPoints, string source)
        {
            return SendCommand(new AddCommand(originPlayerId, targetPlayerId, amountOfPoints, source));
        }

        public Task SendRemove(string originPlayerId, string targetPlayerId, int amountOfPoints, string source)
        {
            var warCounterTick = new WarCounterTick
            {
                AmountTaken = amountOfPoints,
                SourceUser = originPlayerId,
                TargetUser = targetPlayerId
            };

            var commandAndWar = new[]
            {
                SendCommand(new RemoveCommand(originPlayerId, targetPlayerId, amountOfPoints, source)),
                SendToSubscription("WarCounter", warCounterTick)
            };

            return Task.WhenAll(commandAndWar);
        }

        private Task SendCommand(object command)
        {
            var message = new Message
            {
                ContentType = "application/json",
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(command))
            };

            return _client.SendAsync(message);
        }

        private Task SendToSubscription(string subscription, object command)
        {
            var message = new Message
            {
                ContentType = "application/json",
                To = subscription,
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(command))
            };

            return _topicClient.SendAsync(message);
        }
    }
}