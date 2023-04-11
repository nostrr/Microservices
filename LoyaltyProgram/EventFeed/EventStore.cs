namespace LoyaltyProgram.EventFeed
{
    public class EventStore : IEventStore
    {
        private static long currentSequenceNumber = 0;
        private static readonly IList<EventFeedEvent> Database = new List<EventFeedEvent>();
        public Task<IEnumerable<EventFeedEvent>> GetEvents(int start, int end)
            => Task.FromResult<IEnumerable<EventFeedEvent>>(
            Database
            .Where(e => start <= e.SequenceNumber && end >= e.SequenceNumber)
            .OrderBy(e => e.SequenceNumber));
        

        public Task RaiseEvent(string name, object content)
        {
            var seqNumber = Interlocked.Increment(ref currentSequenceNumber);
            Database.Add(new EventFeedEvent(seqNumber, DateTimeOffset.UtcNow, name, content));
            return Task.CompletedTask;
        }
    }

}
