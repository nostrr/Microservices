using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiGatewayMock.ServicesClients
{
    public class LoyaltyProgramClient
    {
        private static readonly IAsyncPolicy<HttpResponseMessage> ExponentialRetryPolicy =
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrTransientHttpStatusCode()
                .WaitAndRetryAsync(
                3,
                attemp =>
                TimeSpan.FromMilliseconds(100 * Math.Pow(2, attemp)));

        private readonly IHttpClientFactory _httpClientFactory;
        private Uri _hostName;


        public LoyaltyProgramClient(
            string loyalProgramMicroserviceHostName,
        IHttpClientFactory httpClientFactory)
        {
            _hostName = new Uri($"http://{loyalProgramMicroserviceHostName}");
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HttpResponseMessage> RegisterUser(string name)
        {
            var user = new { name, Settings = new { } };
            return await ExponentialRetryPolicy.ExecuteAsync(async () =>
            {
                var client = await _httpClientFactory.Create(_hostName, "loyalty_program_write");
                return await client.PostAsync("/users/", CreateBody(user));
            });
        }

        private static StringContent CreateBody(object user) =>
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");


        public async Task<HttpResponseMessage> QueryUser(string arg)
        {
            return await ExponentialRetryPolicy.ExecuteAsync(async () =>
            {
                var client = await _httpClientFactory.Create(_hostName, "loyalty_program_write");
                return await client.GetAsync($"/users/{int.Parse(arg)}");
            });
        }
            

        public async Task<HttpResponseMessage> UpdateUser(LoyaltyProgramUser user)
        {
            return await ExponentialRetryPolicy.ExecuteAsync(async () =>
            {
                var client = await _httpClientFactory.Create(_hostName, "loyalty_program_write");
                return await client.PutAsync($"/users/{user.Id}", CreateBody(user));
            });
        }
    }
}
