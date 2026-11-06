using Azure.Data.Tables;
using AgriStream.Web.Models;

namespace AgriStream.Web.Services
{
    public class TelemetryService
    {
        private readonly TableClient _table;

        public TelemetryService(IConfiguration config)
        {
            var conn = config["StorageConnection"]
                ?? throw new InvalidOperationException("StorageConnection is not configured.");
            _table = new TableClient(conn, "SensorReadings");
            _table.CreateIfNotExists();
        }

        public Task AddReadingAsync(SensorReading reading) => _table.UpsertEntityAsync(reading);

        
        public async Task AddBatchAsync(IEnumerable<SensorReading> readings)
        {
            foreach (var group in readings.GroupBy(r => r.PartitionKey))
            {
                foreach (var chunk in group.Chunk(100))
                {
                    var actions = chunk.Select(r => new TableTransactionAction(TableTransactionActionType.UpsertReplace, r));
                    await _table.SubmitTransactionAsync(actions);
                }
            }
        }

        public async Task<List<SensorReading>> GetLatestAsync(string fieldId, int take = 20)
        {
            var result = new List<SensorReading>();
            await foreach (var r in _table.QueryAsync<SensorReading>(x => x.PartitionKey == fieldId))
            {
                result.Add(r);
                if (result.Count >= take) break;
            }
            return result;
        }
    }
}