using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyProgram.EventFeed
{
    [Route("/events")]
    public class EventFeedController : ControllerBase
    {
        private readonly IEventStore _eventStore;

        public EventFeedController(IEventStore eventStore) => _eventStore = eventStore;

        [HttpGet("")]
        public async Task<ActionResult<EventFeedEvent[]>> GetEvents([FromQuery] int start, [FromQuery] int end)
        {
            if (start < 0 || end < start)
            {
                return BadRequest();
            }
            return (await _eventStore.GetEvents(start, end)).ToArray();
        }
    }
}
