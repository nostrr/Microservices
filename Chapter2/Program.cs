using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using Polly.Bulkhead;
using Serilog;
using Serilog.Events;
using Serilog.Enrichers.Span;
using Serilog.Sinks.SystemConsole;
using ShoppingCart;
using ShoppingCart.Cache;
using ShoppingCart.EventFeed;
using ShoppingCart.Infrastructure;
using ShoppingCart.ServicesClients;
using ShoppingCart.ShoppingCart;
using Serilog.Formatting.Json;
using MiddlewarePackage.Monitoring;
using DbHealthCheckAlias = MiddlewarePackage.Monitoring.DbHealthCheck;
using MiddlewarePackage.NetLogging;

namespace Chapter2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<IShoppingCartStore, ShoppingCartStore>();
            builder.Services.AddScoped<ICache, Cache>();
            builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>()
                .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromMinutes(100 * Math.Pow(2,attempt))));
            builder.Services.AddScoped<IEventStore, SqlEventStore>();
            builder.Services.AddBasicHealthChecks();
            builder.Services.AddAdditionStartupHealthChecks<DbHealthCheckAlias>();
            builder.Services.AddControllers();
            builder.Host.UseLogging(); 


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<MonitoringMiddleware>();

            app.UseRouting();

            app.UseCustomHealthChecks();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.Run();
        }
    }
}