using ShoppingCart.EventFeed;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCart
    {
        private readonly HashSet<ShoppingCartItem> _items = new();

        public int? Id { get; set; }
        public int UserId { get; }
        public IEnumerable<ShoppingCartItem> Items => _items;

        public ShoppingCart(int userId) => UserId = userId;

        public ShoppingCart(int? id, int userId, IEnumerable<ShoppingCartItem> items)
        {
            Id = id;
            UserId = userId;
            _items = items.ToHashSet();
        }

        public void AddItems(
            IEnumerable<ShoppingCartItem> shoppingCartItems,
            IEventStore eventStore)
        {
            foreach (var item in shoppingCartItems)
                if (!(_items.Contains(item) && _items.Remove(item) && false) && _items.Add(item))
                {
                    eventStore.Raise("ShoppingCartItemAdded",
                        new { UserId, item });
                }
        }

        public void RemoveItems(int[] productCatalogueIds, IEventStore eventStore)
        {
            if(_items.RemoveWhere(i => productCatalogueIds.Contains(i.ProductCatalogueId)) != 0)
            {
                foreach (var item in _items)
                {
                    eventStore.Raise("ShoppingCartItemRemove", new { UserId, item });
                }
            }
        }
         
    }
}
