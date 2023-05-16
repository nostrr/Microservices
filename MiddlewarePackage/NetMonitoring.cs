using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiddlewarePackage.Monitoring
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app)
        {
            return app
               .UseHealthChecks("/health/startup", new HealthCheckOptions { Predicate = x => x.Tags.Contains("startup") })
               .UseHealthChecks("/health/live", new HealthCheckOptions { Predicate = x => x.Tags.Contains("liveness") });
        }
    }

    public static class ServiceCollectionExtensions
    {
        private const string Liveness = "liveness";
        private const string Startup = "startup";

        public static IServiceCollection AddBasicHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
              .AddCheck("BasicStasrtupHealthCheck", () => HealthCheckResult.Healthy(), tags: new[] { Startup })
              .AddCheck("BasicLivenessHealthCheck", () => HealthCheckResult.Healthy(), tags: new[] { Liveness });

            return services;
        }

        public static IServiceCollection AddAdditionStartupHealthChecks<T>(this IServiceCollection services) where T : class, IHealthCheck
        {
            services.AddHealthChecks().AddCheck<T>(nameof(T), tags: new[] { Startup });
            return services;
        }

        public static IServiceCollection AddAdditionLivenessHealthChecks<T>(this IServiceCollection services) where T : class, IHealthCheck
        {
            services.AddHealthChecks().AddCheck<T>(nameof(T), tags: new[] { Liveness });
            return services;
        }
    }

    public class DbHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public DbHealthCheck(ILoggerFactory logger)
        {
            _httpClient = new HttpClient();
            _logger = logger.CreateLogger<DbHealthCheck>();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {

            await using var conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;
Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;");
            var result = await conn.QuerySingleAsync<int>("Select 1");
            return result == 1
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Degraded();
        }
    }

}
