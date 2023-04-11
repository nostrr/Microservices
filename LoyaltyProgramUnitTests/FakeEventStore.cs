using LoyaltyProgram.EventFeed;

namespace LoyaltyProgramUnitTests
{
    public class FakeEventStore : IEventStore
    {
        public Task RaiseEvent(string name, object content) =>
          throw new NotImplementedException();

        public Task<IEnumerable<EventFeedEvent>> GetEvents(int start, int end)
        {
            if (start > 100)
                return Task.FromResult(Enumerable.Empty<EventFeedEvent>());

            return Task.FromResult(Enumerable
              .Range(start, end - start)
              .Select(i => new EventFeedEvent(i, DateTimeOffset.UtcNow, "some event", new object())));
        }
    }
}
