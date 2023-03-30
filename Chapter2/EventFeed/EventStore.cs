namespace ShoppingCart.EventFeed
{
    public class EventStore : IEventStore
    {
        private static long currentSequenceNumber = 0;
        private static readonly IList<Event> Database = new List<Event>();
        public async Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber)
        {
            return Database.Where(e =>
            e.SequenctNumber >= firstEventSequenceNumber &&
            e.SequenctNumber <= lastEventSequenceNumber)
                .OrderBy(e => e.SequenctNumber);
        }

        public async Task Raise(string eventName, object content)
        {
            var seqNumber = Interlocked.Increment(ref currentSequenceNumber);
            Database.Add(
                new Event(
                    seqNumber,
                    DateTimeOffset.UtcNow,
                    eventName,
                    content));
        }
    }
}
