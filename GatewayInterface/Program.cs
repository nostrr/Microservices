using MiddlewarePackage.Monitoring;
using MiddlewarePackage.NetLogging;
using Polly;

namespace GatewayInterface
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllersWithViews();
            builder.Host.UseLogging();
            builder.Services.AddBasicHealthChecks();
            builder.Services.AddHttpClient("ProductCatalogClient", client => client.BaseAddress = new Uri("https://localhost:7200"))
                .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt))));
            builder.Services.AddHttpClient("ShoppingCartClient", client => client.BaseAddress = new Uri("https://localhost:7038"));
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseRouting();
            app.UseCustomHealthChecks();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}