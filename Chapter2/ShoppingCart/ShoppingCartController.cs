using Microsoft.AspNetCore.Mvc;
using ShoppingCart.EventFeed;
using ShoppingCart.ServicesClients;

namespace ShoppingCart.ShoppingCart
{
    [Route("/shoppingcart")]
    public class ShoppingCartController //: ControllerBase
    {
        private readonly IShoppingCartStore _shoppingCartStore;
        private readonly IProductCatalogClient _productCatalog;
        private readonly IEventStore _eventStore;
        public ShoppingCartController(
            IShoppingCartStore shoppingCartStore,
            IProductCatalogClient productCatalog,
            IEventStore eventStore) {
            _shoppingCartStore = shoppingCartStore;
            _productCatalog = productCatalog;
            _eventStore = eventStore;
        }

        [HttpGet("{userId:int}")]
        public Task<ShoppingCart> Get(int userId)
        {
            return _shoppingCartStore.Get(userId);
        }

        [HttpPost("{userId:int}/items")]
        public async Task<ShoppingCart> Post(int userId, [FromBody] int[] productIds)
        {
            var shoppingCart = await _shoppingCartStore.Get(userId);
            var shoppingCartItems = await _productCatalog.GetShoppingCartItemsAync(productIds);
            shoppingCart.AddItems(shoppingCartItems, _eventStore);
            await _shoppingCartStore.Save(shoppingCart);
            return shoppingCart;
        }

        [HttpDelete("{userId:int}/items")]
        public async Task<ShoppingCart> Delete(int userId, [FromBody] int[] productIds)
        {
            var shoppingCart = await _shoppingCartStore.Get(userId);
            shoppingCart.RemoveItems(productIds, _eventStore);
            await _shoppingCartStore.Save(shoppingCart);
            return shoppingCart;
        }
    }
}
