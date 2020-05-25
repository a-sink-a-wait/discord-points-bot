using System.Text.Json;

namespace Function.Command.Events
{
    public class EventFactory
    {
        public GameEvent Create(JsonDocument document)
        {
            var action = document.RootElement.GetProperty("Action").GetString();
            var payload = document.RootElement.GetProperty("Payload");

            var pointsPayload = JsonSerializer.Deserialize<PointsCommand>(payload.GetRawText());
            return new GameEvent
            {
                PointsEvent = new PointsEvent
                {
                    OriginPlayerId = pointsPayload.OriginPlayerId,
                    TargetPlayerId = pointsPayload.TargetPlayerId,
                    Action = action.ToLowerInvariant(),
                    Amount = pointsPayload.AmountOfPoints
                }
            };
        }
    }
}