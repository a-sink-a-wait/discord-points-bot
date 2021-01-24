using Microsoft.WindowsAzure.Storage.Table;

namespace Function
{
    public class PlayerPoints : TableEntity
    {
        public string Source => this.PartitionKey;

        public string PlayerName => this.RowKey;

        public int TotalPoints { get; set; }

        public int LastEventIndex { get; set; }
    }
}