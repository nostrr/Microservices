using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog
{
    [Route("/products")]
    public class ProductCatalogController : ControllerBase
    {
        private readonly IProductStore _productStore;
        public ProductCatalogController(IProductStore productStore)
        {
            _productStore = productStore;
        }

        [HttpGet("")]
        [ResponseCache(Duration = 86400)]
        public IEnumerable<ProductCatalogProduct> Get([FromQuery] string productIds)
        {
            var products = _productStore.GetProductsByIds(ParseProductIdsFromQueryString(productIds));
            return products;
        }

        private static IEnumerable<int> ParseProductIdsFromQueryString(string productIdsString) =>
            productIdsString.Split(',').Select(s => s.Replace("[", "").Replace("]", "")).Select(int.Parse);

    }
}
