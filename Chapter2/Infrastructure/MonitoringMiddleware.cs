using System.Diagnostics;

namespace ShoppingCart.Infrastructure
{
    public class MonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public MonitoringMiddleware(RequestDelegate next, ILogger<MonitoringMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogError($"My record {Activity.Current.Id}");
            if (context.Request.Path.Equals("/_monitor/shallow"))
            {
                await ShallowEndpoint(context);
            }
            else
            {
                await _next(context);
            }
        }

        private Task ShallowEndpoint(HttpContext context) 
        {
            context.Response.StatusCode = 204;
            return Task.FromResult(0);
        }
    }
}
