﻿using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly HttpClient _httpClient;
        public LoyaltyProgramClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> RegisterUser(string name)
        {
            var user = new { name, Settings = new { } };
            return await ExponentialRetryPolicy.ExecuteAsync(() =>
            _httpClient.PostAsync("/users/", CreateBody(user))
            );
        }

        private static StringContent CreateBody(object user) =>
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");


        public async Task<HttpResponseMessage> QueryUser(string arg) =>
            await _httpClient.GetAsync($"/users/{int.Parse(arg)}");

        public async Task<HttpResponseMessage> UpdateUser(LoyaltyProgramUser user) =>
            await _httpClient.PutAsync($"/users/{user.Id}", CreateBody(user));
    }
}