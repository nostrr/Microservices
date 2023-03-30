using System.Net.Http.Headers;
using System.Text.Json;

namespace EventConsumer
{
    internal class Program 
    {
        static long start;
        static async Task Main(string[] args)
        {
            var specialOfferSubsriber = new EventSubscriber("http://special-offers:5002");
            specialOfferSubsriber.Start();

            //////////////////////////////////////////////////////Two different aproachs

            start = await GetStartIdFromDatastore();
            var end = 100;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var resp = await client.GetAsync(new Uri($"http://special-offers:5002/events?start={start}&end={end}"));
            await ProcessEvents(await resp.Content.ReadAsStreamAsync());
            await SaveStartIdToDataStore(start);
        }

        // fake implementation. Should get from a real database
        static Task<long> GetStartIdFromDatastore() => Task.FromResult(0L);
       
        // fake implementation. Should apply business rules to events
        static async Task ProcessEvents(Stream content)
        {
            var events = await JsonSerializer.DeserializeAsync<SpecialOfferEvent[]>(content) ?? new SpecialOfferEvent[0];
            foreach (var @event in events)
            {
                Console.WriteLine(@event);
                start = Math.Max(start, @event.SequenceNumber + 1);
            }
        }

        static Task SaveStartIdToDataStore(long startId) => Task.CompletedTask;

    }
}