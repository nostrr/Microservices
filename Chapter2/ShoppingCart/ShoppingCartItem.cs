namespace ShoppingCart.ShoppingCart
{
    public record ShoppingCartItem(
    int ProductId,
    string ProductName,
    string Description,
    Money Price)
    {
        public virtual bool Equals(ShoppingCartItem? obj) =>
          obj != null && this.ProductId.Equals(obj.ProductId);

        public override int GetHashCode() => this.ProductId.GetHashCode();
    }
}
