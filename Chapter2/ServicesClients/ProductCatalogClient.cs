using ShoppingCart.Cache;
using ShoppingCart.ShoppingCart;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ShoppingCart.ServicesClients
{
    public class ProductCatalogClient : IProductCatalogClient
    {
        private readonly HttpClient _client;
        private readonly ICache _cache;
        //private static readonly string ProductCatalogueBaseUrl = @"https://git.io//JeHiE";
        private static readonly string ProductCatalogueBaseUrl = @"https://localhost:7200/products/";
        private static readonly string GetProductPathTemplate = "?productIds=[{0}]";

        public ProductCatalogClient(HttpClient client, ICache cache)
        {
            client.BaseAddress = new Uri(ProductCatalogueBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _client = client;
            _cache = cache;
        }

        public async Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItemsAync(int[] productCatalogueIds)
        {
            var productCatalogItems = await RequestProductFromProductCatalogueAsync(productCatalogueIds);
            return ConvertToShoppingCartItems(productCatalogItems);
        }

        private async Task<IEnumerable<ProductCatalogProduct>> RequestProductFromProductCatalogueAsync(int[] productCatalogueIds)
        {
            var productResource = string.Format(GetProductPathTemplate, string.Join(",", productCatalogueIds));
            var productCatalogItems = _cache.Get(productResource) as IEnumerable<ProductCatalogProduct>;
            if (productCatalogItems is null)
            {
                var response = await _client.GetAsync(productResource);
                productCatalogItems = await GetContentFromResponseAsync(response);
                AddToCache(productResource, response, productCatalogItems);
            }
            return productCatalogItems;
        }

        private void AddToCache(string resource, HttpResponseMessage response, IEnumerable<ProductCatalogProduct> products)
        {
            if (response.IsSuccessStatusCode)
            {
                CacheControlHeaderValue? cacheControl = response?.Headers?.CacheControl;
                if (cacheControl != null)
                {
                    _cache.Add(resource, products, cacheControl.MaxAge!.Value);
                }
            }
        }

        private async Task<IEnumerable<ProductCatalogProduct>> GetContentFromResponseAsync(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStreamAsync();
            var products = await JsonSerializer.DeserializeAsync<List<ProductCatalogProduct>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ??
                new();
            return products;
        }

        private static IEnumerable<ShoppingCartItem> ConvertToShoppingCartItems(IEnumerable<ProductCatalogProduct> products)
        {
            return products.Select(p =>
            new ShoppingCartItem(
                p.ProductId,
                p.ProductName,
                p.ProductDescription,
                p.Price));
        }

        private record ProductCatalogProduct(int ProductId, string ProductName, string ProductDescription, Money Price);
    }

}
