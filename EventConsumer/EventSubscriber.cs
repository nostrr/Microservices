using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using Timer = System.Timers.Timer;

namespace EventConsumer
{
    public class EventSubscriber
    {
        private readonly string _specialOffersHost;
        private long _start = 0, _chunkSize = 100;
        private readonly Timer _timer;

        public EventSubscriber(string specialOffersHost)
        {
            WriteLine("created");
            _specialOffersHost = specialOffersHost;
            _timer = new Timer(10 * 1000);
            _timer.AutoReset = false;
            _timer.Elapsed += (_, __) => SubscriptionCycleCallback().Wait();
        }

        private async Task SubscriptionCycleCallback()
        {
            var response = await ReadEvents();
            if (response.StatusCode == HttpStatusCode.OK)
                HandleEvents(await response.Content.ReadAsStringAsync());
            _timer.Start();
        }

        private async Task<HttpResponseMessage> ReadEvents()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri($"http://{_specialOffersHost}");
                var response = await httpClient.GetAsync($"/events/?start={_start}&end={_start + _chunkSize}");
                PrettyPrintResponse(response);
                return response;
            }
        }

        private void HandleEvents(string content)
        {
            WriteLine("Handling events");
            var events = JsonConvert.DeserializeObject<IEnumerable<SpecialOfferEvent>>(content);
            WriteLine(events);
            WriteLine(events.Count());
            foreach (var ev in events)
            {
                WriteLine(ev.Content);
                dynamic eventData = ev.Content;
                WriteLine("product name from data: " + (string)eventData.item.productName);
                _start = Math.Max(_start, ev.SequenceNumber + 1);
            }
        }


        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private static async void PrettyPrintResponse(HttpResponseMessage response)
        {
            WriteLine("Status code: " + response?.StatusCode.ToString() ?? "command failed");
            WriteLine("Headers: " + response?.Headers.Aggregate("", (acc, h) => acc + "\n\t" + h.Key + ": " + h.Value) ?? "");
            WriteLine("Body: " + await response?.Content.ReadAsStringAsync() ?? "");
        }
    }
}
