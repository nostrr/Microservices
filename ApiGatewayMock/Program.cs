using ApiGatewayMock.ServicesClients;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using System.Text.Json;
using static System.Console;
using static System.Environment;
namespace ApiGatewayMock
{
    public class Program
    {
        private static string _host;
        private static LoyaltyProgramClient _client;

        private static AsyncPolicy<HttpResponseMessage> exponentialRetryPolicy = 
            Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrTransientHttpStatusCode()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));

        private static AsyncPolicy<HttpResponseMessage> circuitBreakerPolicy =
          Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrTransientHttpStatusCode()
            .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1));

        private static T GetClient<T>() where T : class
        {
            var serviceProvider = new ServiceCollection()
               .AddHttpClient<T>()
               .AddPolicyHandler(request =>
                   request.Method == HttpMethod.Get
                   ? circuitBreakerPolicy
                   : exponentialRetryPolicy)
               .ConfigureHttpClient(c => c.BaseAddress = new Uri(_host))
               .Services
               .BuildServiceProvider();
          return  serviceProvider.GetService<T>();
                
        }

        static async Task Main(string[] args)
        {
            _host = args.Length > 0 ? args[0] : "localhost:7270";
            _client = new LoyaltyProgramClient(_host, new HttpClientFactory());
            var proccesCommand = new Dictionary<char, (string description, Func<string, Task<(bool, HttpResponseMessage)>> handler)>
            {
                {
                    'r',
                    ("r <user name> - to register a user with name <user name> with the Loyalty Program Microservice.",
                    async c => (true, await _client.RegisterUser(c.Substring(2))))
                },
                {
                    'q',
                    ("q <userid> - to query the Loyalty Program Microservice for a user with id <userid>.",
                    async c => (true, await _client.QueryUser(c.Substring(2))))
                },
                {
                    'u',
                      ("u <userid> <interests> - to update a user with new interests",
                     HandleUpdateInterestsCommand)
                },
                {
                    'x',
                    ("x - exit",
                    _ => Task.FromResult((false, new HttpResponseMessage(0))))
                }
            };

            WriteLine("Welcome to the API Gatway Mock.");

            var cont = true;
            while (cont)
            {
                WriteLine();
                WriteLine();
                WriteLine("*****************");
                WriteLine("Choose one of:");
                foreach (var c in proccesCommand.Values)
                    WriteLine(c.description);
                WriteLine("****************");
                var cmd = ReadLine() ?? string.Empty;
                if (proccesCommand.TryGetValue(cmd[0], out var command))
                {
                    var (@continue, response) = await command.handler(cmd);
                    await PrettyPrint(response);
                    cont = @continue;
                }

            }

        }

        static async Task<(bool, HttpResponseMessage)> HandleUpdateInterestsCommand(string cmd)
        {
            var response = await _client.QueryUser(cmd.Split(' ').Skip(1).First());
            if (!response.IsSuccessStatusCode)
                return (true, response);
            var user = JsonSerializer.Deserialize<LoyaltyProgramUser>(await response.Content.ReadAsStringAsync(), 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (user is null)
                return (true, response);
            var newInterests = cmd[cmd.IndexOf(' ', 2)..].Split(',').Select(i => i.Trim());
            var res = await _client.UpdateUser(
              user with
              {
                  Settings = user.Settings with
                  {
                      Interests = user.Settings.Interests.Concat(newInterests).ToArray()
                  }
              });
            return (true, res);
        }

        static async Task PrettyPrint(HttpResponseMessage response)
        {
            if (response.StatusCode == 0) return;
            WriteLine("********** Response **********");
            WriteLine($"status code: {response.StatusCode}");
            WriteLine("Headers: " + response.Headers.Aggregate("",
              (acc, h) => $"{acc}{NewLine}\t{h.Key}: {h.Value.Aggregate((hAcc, hVal) => $"{hAcc}{hVal}, ")}") ?? "");
            if (response.Content.Headers.ContentLength > 0)
                WriteLine(@$"Body:{NewLine}{JsonSerializer.Serialize(await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync()), new JsonSerializerOptions { WriteIndented = true })}");
        }
    }
}
