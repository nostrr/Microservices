namespace ProductCatalog
{
    public class ProductStore : IProductStore
    {
        public IEnumerable<ProductCatalogProduct> GetProductsByIds(IEnumerable<int> productIds) =>
          productIds.Select(id => new ProductCatalogProduct(id, "foo" + id, "bar", new Money("euro", new Random(DateTime.Now.Millisecond).Next())));
    }
}
