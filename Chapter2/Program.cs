using Polly;
using ShoppingCart.Cache;
using ShoppingCart.EventFeed;
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
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());


           // app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}