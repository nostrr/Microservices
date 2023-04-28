namespace ShoppingCart.Infrastructure
{
    public class MyMiddleware
    {
        private readonly RequestDelegate next;

        public MyMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query.Any(x => x.Value == "Nostradamus"))
            {

                context.Response.StatusCode = 500;
            }
            else
            {
                await next(context);
            }

        }
    }
}