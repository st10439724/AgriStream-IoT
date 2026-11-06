using Azure;
using Azure.Data.Tables;

namespace AgriStream.Web.Models
{
    public class SensorReading : ITableEntity
    {
        
        public string PartitionKey { get; set; } = string.Empty;   
        public string RowKey { get; set; } = string.Empty;         
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        
        public string DeviceId { get; set; } = string.Empty;
        public double SoilMoisture { get; set; }
        public double Temperature { get; set; }

        public SensorReading()
        {
        }

        public SensorReading(string fieldId, string deviceId, double soilMoisture, double temperature)
        {
            PartitionKey = fieldId;
            RowKey = BuildRowKey(DateTime.UtcNow);
            DeviceId = deviceId;
            SoilMoisture = soilMoisture;
            Temperature = temperature;
        }

        
        public static string BuildRowKey(DateTime utcTime)
        {
            return (DateTime.MaxValue.Ticks - utcTime.Ticks).ToString("D19");
        }
    }
}