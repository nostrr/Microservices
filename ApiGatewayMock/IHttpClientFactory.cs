using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGatewayMock
{
    public interface IHttpClientFactory
    {
        Task<HttpClient> Create(Uri url, string scope);
    }
}
