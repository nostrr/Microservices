using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data.SqlClient;

namespace ShoppingCart
{
    public class DbHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public DbHealthCheck(ILoggerFactory logger)
        {
            _httpClient= new HttpClient();
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
