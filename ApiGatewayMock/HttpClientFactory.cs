using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiGatewayMock
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly ClientCredentialsTokenRequest _clientOptions;

        public HttpClientFactory()
        {
            _clientOptions = new ClientCredentialsTokenRequest
            {
                Address = "http://localhost:5001/connect/token",
                ClientId = "api_gateway",
                ClientSecret = "secret",
                GrantType = "client_credentials"
            };
        }

        public async Task<HttpClient> Create(Uri uri, string scope)
        {
            _clientOptions.Scope = scope;
            var response = await new HttpClient().RequestClientCredentialsTokenAsync(_clientOptions).ConfigureAwait(false);
            var client = new HttpClient() { BaseAddress= uri };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response.AccessToken);
            return client;
        }
    }
}
 