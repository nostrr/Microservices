using Dapper;
using ShoppingCart.ServicesClients;
using System.Data.SqlClient;
using System.Linq;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCartStore : IShoppingCartStore
    {
        private const string CONNECTION_STRING = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ShoppingCart;
Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private const string READ_ITEM_SQL = @"select sc.ID, ProductCatalogId, ProductName, ProductDescription, Currency, Amount from ShoppingCart as sc
join ShoppingCartItem sci on sc.ID = sci.ShoppingCartId
and sc.UserId=@UserId";
        private const string INSERT_SHOPPING_CART_SQL =
           @"insert into ShoppingCart (UserId) OUTPUT inserted.ID VALUES (@UserId)";
        private const string DELETE_ALL_FOR_SHOPPING_CART_SQL =
            @"delete item from ShoppingCartItem item
inner join ShoppingCart cart on item.ShoppingCartId = cart.ID
and cart.UserId=@UserId";
        private const string ADD_ALL_FOR_SHOPPING_CART_SQL =
           @"insert into ShoppingCartItem
(ShoppingCartId, ProductCatalogId, ProductName,
ProductDescription, Amount, Currency)
values
(@ShoppingCartId, @ProductCatalogueId, @ProductName,
@ProductDescription, @Amount, @Currency)";
        public async Task<ShoppingCart> Get(int userId)
        {
            await using var conn = new SqlConnection(CONNECTION_STRING);
            var items = (await conn.QueryAsync(READ_ITEM_SQL, new { UserId = userId })).ToList();
            return new ShoppingCart(
                 items.FirstOrDefault()?.ID,
                 userId,
                 items.Select(x =>
                        new ShoppingCartItem(
                                     (int)x.ProductCatalogId,
                                      x.ProductName,
                                      x.ProductDescription,
                                      new Money(x.Currency, x.Amount))));
        }

        public async Task Save(ShoppingCart shoppingCart)
        {
            await using var conn = new SqlConnection(CONNECTION_STRING);
            await conn.OpenAsync();
            await using (var tx = conn.BeginTransaction())
            {
                var shoppingCartId =
                    shoppingCart.Id ?? await conn.QuerySingleAsync<int>(INSERT_SHOPPING_CART_SQL, new { shoppingCart.UserId }, tx);

                await conn.ExecuteAsync(
                    DELETE_ALL_FOR_SHOPPING_CART_SQL,
                    new { UserId = shoppingCart.UserId },
                    tx);

                await conn.ExecuteAsync(
                    ADD_ALL_FOR_SHOPPING_CART_SQL,
                    shoppingCart.Items.Select(x =>
                              new
                              {
                                  shoppingCartId,
                                  x.ProductCatalogueId,
                                  Productdescription = x.Description,
                                  x.ProductName,
                                  x.Price.Amount,
                                  x.Price.Currency
                              }),
                               tx);
                await tx.CommitAsync();
                shoppingCart.Id = shoppingCartId;
            }
        }
    }
}
