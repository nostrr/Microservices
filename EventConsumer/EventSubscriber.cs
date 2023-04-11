using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
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
        private IDictionary<long, SpecialOfferEvent> database = new ConcurrentDictionary<long, SpecialOfferEvent>();

        public EventSubscriber(string specialOffersHost)
        {
            WriteLine("created");
            _specialOffersHost = specialOffersHost;
            _timer = new Timer(10 * 1000);
            _timer.AutoReset = false;
            _timer.Elapsed += (_, __) =>
            {
                SubscriptionCycleCallback().Wait();
            };
        }

        private async Task SubscriptionCycleCallback()
        {
            var response = await ReadEvents().ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK)
                HandleEvents(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            _timer.Start();
        }

        private async Task<HttpResponseMessage> ReadEvents()
        {
            _start = await ReadStartNumber().ConfigureAwait(false);
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri($"https://{_specialOffersHost}");
                var response = await httpClient.GetAsync($"/events/?start={_start}&end={_start + _chunkSize}");
                PrettyPrintResponse(response);
                return response;
            }
        }

        private async Task<long> ReadStartNumber()
        {
            return await Task.Run(() => database.Any() ? database.Keys.Max() + 1 : 1);
        }

        private void HandleEventsConsole(string content)
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

        private async void HandleEvents(string content)
        {
            var lastSucceededEvent = 0L;
            var events = JsonConvert.DeserializeObject<IEnumerable<SpecialOfferEvent>>(content);
            foreach (var ev in events)
            {
                dynamic eventData = ev.Content;
                if (ShouldSendNotification(eventData))
                {
                    var notificationSucceeded = await
                        SendNotification(eventData).ConfigureAwait(false);
                    if (!notificationSucceeded)
                        return;
                }
                lastSucceededEvent = ev.SequenceNumber + 1;
            }
            await WriteStartNumber(lastSucceededEvent).ConfigureAwait(false);
        }

        private async Task WriteStartNumber(long lastSucceededEvent)
        {
            await Task.Run(() =>
             {
                 _start = lastSucceededEvent;
             });
        }

        private async Task SendNotification(dynamic eventData)
        {
            await Task.Run(() => Console.WriteLine(eventData.Name));
        }

        private bool ShouldSendNotification(dynamic eventData)
        {
            if(string.IsNullOrEmpty(eventData.Name))
                return false;
            return true;
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
