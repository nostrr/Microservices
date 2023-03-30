namespace ShoppingCart.EventFeed
{
    public record Event(long SequenctNumber, DateTimeOffset OccuredAt, string Name, object Content);
}
