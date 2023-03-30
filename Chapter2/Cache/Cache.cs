using System.Collections.Concurrent;

namespace ShoppingCart.Cache
{
    public class Cache : ICache
    {
        private static readonly IDictionary<string, (DateTimeOffset, object)> cache =
         new ConcurrentDictionary<string, (DateTimeOffset, object)>();

        public void Add(string key, object value, TimeSpan ttl) =>
            cache[key] = (DateTimeOffset.UtcNow.Add(ttl), value);

        public object? Get(string productsResource)
        {
            if (cache.TryGetValue(productsResource, out var value)
                 && value.Item1 > DateTimeOffset.UtcNow)
                return value.Item2;
            cache.Remove(productsResource);
            return null;
        }
    }
}
