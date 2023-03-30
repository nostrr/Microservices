using ShoppingCart.ShoppingCart;

namespace ShoppingCart.ServicesClients
{
    public interface IProductCatalogClient
    {
        Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItemsAync(int[] productCatalogueIds);
    }
}
