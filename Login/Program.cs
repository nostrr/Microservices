using IdentityModel;
using IdentityServer4.Models;
using Login.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography.X509Certificates;

namespace Login
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Debug));

            // Add services to the container.
          //  builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var cert = new X509Certificate2(Path.Combine(builder.Environment.ContentRootPath, "idsrv3test.pfx"), "alex1234");
            builder.Services
                   .AddIdentityServer()
                   .AddSigningCredential(cert)
                   .AddInMemoryApiScopes(new[] {new ApiScope("loyalty_program_write", "Loyalty Program Write Access"),  })
                   //.AddInMemoryApiResources(new[] { new ApiResource("loyalty_program_write", "Loyalty Program write access") {
                   //          Scopes = { "loyalty_program_write" },
                   //          UserClaims = new List<string> { "write:loyalty_program" }
                   //} })
                   .AddInMemoryClients(Clients.Get());

            builder.Services.AddAuthentication("Temp")
            .AddCookie("Temp", options =>
            {
                options.Cookie.Name = "Temp";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            });

            builder.Services.AddMvc();

            // Build the application.
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseAuthentication();

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Start the application.
            app.Run("http://localhost:5001");
        }
    }
}
