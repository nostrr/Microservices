using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventConsumer
{
    public static class EventConsumer
    {
        public static async Task ConsumeBatch(int start, int end, string specialOffersHostName, string notificationsHostName)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var resp = await client.GetAsync(new Uri($"{specialOffersHostName}/events?start={start}&end={end}"));

            var events = await JsonSerializer.DeserializeAsync<dynamic[]>(await resp.Content.ReadAsStreamAsync()) ?? Array.Empty<dynamic>();

            foreach (var @event in events)
            {
                await client.PostAsync($"{notificationsHostName}/notify", new StringContent(""));
            }
        }
    }
}
