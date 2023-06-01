using Microsoft.AspNetCore.Http;

namespace CustomMiddleware
{
    public class ConsoleMiddleware
    {
        private readonly RequestDelegate next;

        public ConsoleMiddleware(RequestDelegate next) => this.next = next;

        public Task Invoke(HttpContext ctx)
        {
            Console.WriteLine("Got request in class middleware");
            return this.next(ctx);
        }
    }

    public class RedirectingMiddleware
    {
        private readonly RequestDelegate next;

        public RedirectingMiddleware(RequestDelegate next) => this.next = next;

        public Task Invoke(HttpContext ctx)
        {
            switch (ctx.Request.Path.Value?.TrimEnd('/'))
            {
                case "/oldpath":
                    {
                        ctx.Response.Redirect("/newpath", permanent: true);
                        return Task.CompletedTask;
                    }
                default:
                    return this.next(ctx);
            }
        }
    }
}