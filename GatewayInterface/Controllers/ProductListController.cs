using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GatewayInterface.Controllers
{
    public class ProductListController : Controller
    {
        private readonly HttpClient _productCatalogClient;
        private readonly HttpClient _shoppingCartClient;

        public ProductListController(IHttpClientFactory httpClientFactory)
        {
            _productCatalogClient = httpClientFactory.CreateClient("ProductCatalogClient");
            _shoppingCartClient = httpClientFactory.CreateClient("ShoppingCartClient");
        }


        [HttpGet("/productlist")]
        public async Task<IActionResult> ProductList([FromQuery]int userId)
        {
            var products = await GetProductsFromCatalog();
            var cartProducts = await GetProductsFromCart(userId);
            return View(new ProductListViewModel(
              products,
              cartProducts
            ));
        }

        [HttpPost("/shoppingcart/{userId}")]
        public async Task<OkResult> AddToCart(int userId, [FromBody] int productId)
        {
            var response = await _productCatalogClient.PostAsJsonAsync($"/shoppingcart/{userId}/items", new[] { productId });
            response.EnsureSuccessStatusCode();
            return Ok();
        }

        [HttpDelete("/shoppingcart/{userId}")]
        public async Task<OkResult> RemoveFromCart(int userId, [FromBody] int productId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/shoppingcart/{userId}/items");
            request.Content = new StringContent(JsonSerializer.Serialize(new[] { productId }));
            var response = await _productCatalogClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return Ok();
        }

        private async Task<Product[]> GetProductsFromCatalog()
        {
            var response = await _productCatalogClient.GetAsync("/products?productIds=1,2,3,4");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStreamAsync();
            var products =
              await JsonSerializer.DeserializeAsync<Product[]>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return products;
        }
        private async Task<Product[]> GetProductsFromCart(int userId)
        {
            var response = await _shoppingCartClient.GetAsync($"/shoppingcart/{userId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStreamAsync();
            var cart =
              await JsonSerializer.DeserializeAsync<ShoppingCart>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return cart.Items;
        }

    }

    public record Product(int ProductId, string ProductName, string Description);

    public record ShoppingCart(int UserId, Product[] Items);

    public record ProductListViewModel(Product[] Products, Product[] CartProducts);
}
