using EventStore.ClientAPI;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ShoppingCart.EventFeed
{
    public class EsEventStore : IEventStore
    {
        private const string CONNECTION_STRING = "tcp://admin:changeit@localhost:1113";
        public async Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber)
        {
            using var connection = EventStoreConnection.Create(
              ConnectionSettings.Create().DisableTls().Build(),
              new Uri(CONNECTION_STRING));
            await connection.ConnectAsync();
            var result = await connection.ReadStreamEventsForwardAsync(
              "ShoppingCart",
              start: firstEventSequenceNumber,
              count: (int)(lastEventSequenceNumber - firstEventSequenceNumber),
              resolveLinkTos: true);

            return result.Events
              .Select(e =>
                new
                {
                    Content = Encoding.UTF8.GetString(e.Event.Data),
                    Metadata = JsonSerializer.Deserialize<EventMetadata>(Encoding.UTF8.GetString(e.Event.Metadata), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!
                })
              .Select((e, i) =>
                new Event(
                  i + firstEventSequenceNumber,
                  e.Metadata.OccuredAt,
                  e.Metadata.EventName,
                  e.Content));
        }

        public async Task Raise(string eventName, object content)
        {
            using var connection = EventStoreConnection.Create(
              ConnectionSettings.Create().DisableTls().Build(),
              new Uri(CONNECTION_STRING));
            await connection.ConnectAsync();
            var res = await connection.AppendToStreamAsync(
              "ShoppingCart",
              ExpectedVersion.Any,
              new EventData(
                Guid.NewGuid(),
                "ShoppingCartEvent",
                isJson: true,
                data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content)),
                metadata: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new EventMetadata
                (
                  DateTimeOffset.UtcNow,
                  eventName
                )))));
        }
    }

    public record EventMetadata(DateTimeOffset OccuredAt, string EventName);
}
