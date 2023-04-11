using LoyaltyProgram;
using LoyaltyProgram.EventFeed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LoyaltyProgramUnitTests
{
    public class EventFeed_should : IDisposable
    {
        private readonly WebApplicationFactory<Program> _host;
        private readonly HttpClient _sut;

        public EventFeed_should()
        {
            _host = new WebApplicationFactory<Program>().WithWebHostBuilder(host => {
                host.ConfigureServices(x => x.AddScoped<IEventStore, FakeEventStore>());
            });
            _sut = _host.CreateClient();
        }

        [Fact]
        public async Task return_events_when_from_event_store()
        {
            var actual = await _sut.GetAsync("/events?start=0&end=100");

            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            var eventFeedEvents =
              await JsonSerializer.DeserializeAsync<IEnumerable<EventFeedEvent>>(await actual.Content.ReadAsStreamAsync())
              ?? Enumerable.Empty<EventFeedEvent>();
            Assert.Equal(100, eventFeedEvents.Count());
        }

        [Fact]
        public async Task return_empty_response_when_there_are_no_more_events()
        {
            var actual = await _sut.GetAsync("/events?start=200&end=300");

            var eventFeedEvents = await JsonSerializer.DeserializeAsync<IEnumerable<EventFeedEvent>>(
                await actual.Content.ReadAsStreamAsync());
            Assert.Empty(eventFeedEvents);
        }

        public void Dispose()
        {
            _sut?.Dispose();
            _sut?.Dispose();
        }
    }
}
