namespace PointsBot.Infrastructure.Models
{
    public class WarCounterTick
    {
        public string SourceUser { get; set; }

        public string TargetUser { get; set; }

        public int AmountTaken { get; set; }
    }
}