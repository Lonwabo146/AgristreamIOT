using Azure;
using Azure.Data.Tables;
namespace AgristreamIOT.Models

{
    public class SensorReadings : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Type { get; set; }
        public double Value { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
