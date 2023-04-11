using EventConsumer;
using LoyaltyProgram;
using LoyaltyProgram.Users;
using LoyaltyProgramServiceTests.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LoyaltyProgramServiceTests.Scenarios
{
    public class RegisterUserAndGetNotification : IDisposable
    {
        private static int _mocksPort = 5050;
        private readonly MocksHost _serviceMock;
        private readonly WebApplicationFactory<Program> _loyaltyProgramHost;
        private readonly HttpClient _sut;

        public RegisterUserAndGetNotification()
        {
            _serviceMock = new MocksHost(_mocksPort);
            _loyaltyProgramHost = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(_ => { });
            _sut = _loyaltyProgramHost.CreateClient();
        }

        [Fact]
        public async Task Scenario()
        {
            await RegisterNewUser();
            await RunConsumer();
            AssertNotificationWasSent();
        }

        private async Task RegisterNewUser()
        {
            var actual = await _sut.PostAsync(
              "/users",
              new StringContent(
                JsonSerializer.Serialize(new LoyaltyProgramUser(0, "Chr", 0, new LoyaltyProgramSettings())),
                Encoding.UTF8,
                "application/json"));
            Assert.Equal(HttpStatusCode.Created, actual.StatusCode);
        }

        private async Task RunConsumer() =>
           await EventConsumer.EventConsumer.ConsumeBatch(
                0,
                100,
                $"http://localhost:{_mocksPort}/specialoffers",
                $"http://localhost:{_mocksPort}"
                );

        private void AssertNotificationWasSent() => Assert.True(NotificationsMock.ReceivedNotification);

        public void Dispose()
        {
            _serviceMock?.Dispose();
            _sut?.Dispose();
            _loyaltyProgramHost?.Dispose();
        }
    }
}
