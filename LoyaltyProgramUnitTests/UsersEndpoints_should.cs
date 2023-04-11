using LoyaltyProgram;
using LoyaltyProgram.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace LoyaltyProgramUnitTests
{
    public class UsersEndpoints_should : IDisposable
    {
        private readonly WebApplicationFactory<Program> _webHost;
        private readonly HttpClient _sut;
        public void Dispose()
        {
            _webHost?.Dispose();
            _sut?.Dispose();
        }

        public  UsersEndpoints_should()
        {
            _webHost = new WebApplicationFactory<Program>().WithWebHostBuilder(_ => { });
            _sut = _webHost.CreateClient();
        }

        [Fact]
        public async Task respond_not_fount_when_queried_for_unregistered_user()
        {
            var actual = await _sut.GetAsync("/users/1000");
            Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode);
        }

        [Fact]
        public async Task allow_to_register_new_user()
        {
            var expected = new LoyaltyProgramUser(0, "Christian", 0, new LoyaltyProgramSettings());

            var registrationResponse = await _sut.PostAsync("/users",
                new StringContent(JsonSerializer.Serialize(expected), Encoding.UTF8, "application/json"));

            var newUser = await JsonSerializer.DeserializeAsync<LoyaltyProgramUser>(
                await registrationResponse.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var actual = await _sut.GetAsync($"/users/{newUser?.Id}");
            var actualUser = JsonSerializer.Deserialize<LoyaltyProgramUser>(await actual.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Equal(expected.Name, actualUser?.Name);
        }

        [Fact]
        public async Task allow_modifying_users()
        {
            var expected = "jane";
            var user = new LoyaltyProgramUser(0, "Christian", 0, new LoyaltyProgramSettings());
            var registrationResponse = await _sut.PostAsync(
                "/users",
                new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));
            var newUser = await
                JsonSerializer.DeserializeAsync<LoyaltyProgramUser>(
                    await registrationResponse.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var updateUser = newUser! with { Name = expected };
            var actual = await _sut.PutAsync($"/users/{newUser.Id}",
                new StringContent(JsonSerializer.Serialize(updateUser), Encoding.UTF8, "application/json"));
            var actualUser = await JsonSerializer.DeserializeAsync<LoyaltyProgramUser>(
                await actual.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Equal(expected, actualUser?.Name);
        }
    }
}