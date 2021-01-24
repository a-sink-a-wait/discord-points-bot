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

        public CommandSender(IQueueClient client)
        {
            _client = client;
        }

        public Task SendAdd(string originPlayerId, string targetPlayerId, int amountOfPoints, string source)
        {
            return SendCommand(new AddCommand(originPlayerId, targetPlayerId, amountOfPoints, source));
        }

        public Task SendRemove(string originPlayerId, string targetPlayerId, int amountOfPoints, string source)
        {
            return SendCommand(new RemoveCommand(originPlayerId, targetPlayerId, amountOfPoints, source));
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
    }
}