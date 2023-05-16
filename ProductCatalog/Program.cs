using MiddlewarePackage.NetLogging;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Json;
using System.Diagnostics;

namespace ProductCatalog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<IProductStore, ProductStore>();

            builder.Services.AddControllers();

            builder.Host.UseLogging();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseMiddleware<CheckTraceId>();

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }

    public class CheckTraceId
    {
        private readonly ILogger<CheckTraceId> _logger;
        private readonly RequestDelegate _next;
        public CheckTraceId(RequestDelegate next, ILogger<CheckTraceId> logger)
        {
            _logger = logger;
            _next = next;
        }

        public  async Task InvokeAsync(HttpContext context)
        {
            var traceParent = context.Request.Headers["Traceparent"];
            _logger.LogError($"My record {context.Request.Headers["Traceparent"].ToString()} {Activity.Current.Id}");
            await _next(context);
        }
    }
}