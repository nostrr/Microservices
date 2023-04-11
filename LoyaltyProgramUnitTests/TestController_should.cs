using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace LoyaltyProgramUnitTests
{
    public class TestController_should : IDisposable
    {
        private readonly IHost _host;
        private readonly HttpClient _sut;

        public class TestController : ControllerBase
        {
            [HttpGet("/")]
            public OkResult Get() => Ok();
        }

        public TestController_should()
        {
            _host = Host.CreateDefaultBuilder()
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.UseTestServer();
                 webBuilder.ConfigureServices(services => services.AddControllersByType(typeof(TestController)));
                 webBuilder.Configure(app => app.UseRouting().UseEndpoints(endpoints => endpoints.MapControllers()));
             })
             .Start();

            _sut = _host.GetTestClient();
        }

        [Fact]
        public async Task respond_ok_to_request_to_root()
        {
            var actual = await _sut.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
        }

        public void Dispose()
        {
            _host?.Dispose();
            _sut?.Dispose();
        }
    }
}
