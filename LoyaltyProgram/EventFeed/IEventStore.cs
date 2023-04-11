namespace LoyaltyProgram.EventFeed
{
    public interface IEventStore
    {
        Task RaiseEvent(string name, object content);
        Task<IEnumerable<EventFeedEvent>> GetEvents(int start, int end);
    }
}
