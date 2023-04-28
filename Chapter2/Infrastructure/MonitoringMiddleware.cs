namespace ShoppingCart.Infrastructure
{
    public class MonitoringMiddleware
    {
        private readonly RequestDelegate next;

        public MonitoringMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Equals("/_monitor/shallow"))
            {
                await ShallowEndpoint(context);
            }
            else
            {
                await this.next(context);
            }
        }

        private Task ShallowEndpoint(HttpContext context) 
        {
            context.Response.StatusCode = 204;
            return Task.FromResult(0);
        }
    }
}
