using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyProgramServiceTests.Mocks
{
    public class MocksHost : IDisposable
    {
        private readonly IHost _hostForMocks;

        public MocksHost(int port)
        {
            _hostForMocks =
              Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(x => x
                  .ConfigureServices(services => services.AddControllers())
                  .Configure(app => app.UseRouting().UseEndpoints(opt => opt.MapControllers()))
                  .UseUrls($"http://localhost:{port}"))
                .Build();

            new Thread(() => _hostForMocks.Run()).Start();
        }

        public void Dispose()
        {
            _hostForMocks.Dispose();
        }
    }
    
}
