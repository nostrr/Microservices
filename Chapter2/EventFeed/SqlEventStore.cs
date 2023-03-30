using Dapper;
using System.Data.SqlClient;
using System.Text.Json;

namespace ShoppingCart.EventFeed
{
    public class SqlEventStore : IEventStore
    {
        private const string CONNECTION_STRING = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ShoppingCart;
Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private const string WRITE_EVENT_SQL=
          @"insert into EventStore(Name, OccurredAt, Content) values (@Name, @OccurredAt, @Content)";
        private const string READ_EVENT_SQL=
    @"select * from EventStore where ID >= @Start and ID <= @End";

        public async Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber)
        {
            await using var conn = new SqlConnection(CONNECTION_STRING);
            return await conn.QueryAsync<Event>(
                READ_EVENT_SQL,
                new
                {
                    Start = firstEventSequenceNumber,
                    End = lastEventSequenceNumber
                });
        }

        public async Task Raise(string eventName, object content)
        {
            var jsonContent = JsonSerializer.Serialize(content);
            await using var conn = new SqlConnection(CONNECTION_STRING);
            await conn.ExecuteAsync(
                WRITE_EVENT_SQL,
                new
                {
                    Name = eventName,
                    OccurredAt = DateTimeOffset.Now,
                    Content = jsonContent
                });

        }
    }
}
