using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using ShoppingCart;
using ShoppingCart.Cache;
using ShoppingCart.EventFeed;
using ShoppingCart.Infrastructure;
using ShoppingCart.ServicesClients;
using ShoppingCart.ShoppingCart;

namespace Chapter2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<IShoppingCartStore, ShoppingCartStore>();
            builder.Services.AddScoped<ICache, Cache>();
            //  builder.Services.AddScoped<IProductCatalogClient, ProductCatalogClient>(p => new ProductCatalogClient(new HttpClient()));
            builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>()
                .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromMinutes(100 * Math.Pow(2,attempt))));
            builder.Services.AddScoped<IEventStore, SqlEventStore>();
            builder.Services.AddHealthChecks()
                .AddCheck("LivenessHealthCheck", () => HealthCheckResult.Healthy(), tags: new[] { "liveness" })
                .AddCheck<DbHealthCheck>(nameof(DbHealthCheck), tags: new[] { "startup" });
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<MonitoringMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseHealthChecks("/health/live", new HealthCheckOptions { Predicate = x => x.Tags.Contains("liveness") });
            app.UseHealthChecks("/health/startup", new HealthCheckOptions { Predicate = x => x.Tags.Contains("startup") });
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.MapRazorPages();

            app.Run();
        }
    }
}