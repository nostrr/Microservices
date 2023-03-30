using Microsoft.AspNetCore.Mvc;

namespace ShoppingCart.EventFeed
{
    [Route("/events")]
    public class EventFeedController
    {
        private readonly IEventStore _eventStore;

        public EventFeedController(IEventStore eventStore) => _eventStore = eventStore;

        [HttpGet("")]
        public async Task<Event[]> Get([FromQuery] long start, [FromQuery] long end = long.MaxValue)
        {
           return (await _eventStore.GetEvents(start, end)).ToArray();
        }
    }
}
