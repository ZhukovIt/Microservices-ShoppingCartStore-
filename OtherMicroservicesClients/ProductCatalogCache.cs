using CSharpFunctionalExtensions;
using ProductCatalogLogic.ProductCatalog;

namespace Api.OtherMicroservicesClients
{
    public sealed class ProductCatalogCache
    {
        private readonly Dictionary<long, Tuple<Product, DateTimeOffset>> _products = new();

        public void Add(long key, Product value, TimeSpan ttl)
        {
            _products[key] = Tuple.Create(value, DateTimeOffset.UtcNow.Add(ttl));
        }

        public Maybe<Product> Get(long key)
        {
            ClearOld();

            if (!_products.ContainsKey(key))
                return Maybe.None;

            return _products[key].Item1.AsMaybe();
        }

        private void ClearOld()
        {
            foreach (KeyValuePair<long, Tuple<Product, DateTimeOffset>> pair in _products)
            {
                if (DateTimeOffset.UtcNow > pair.Value.Item2)
                    _products.Remove(pair.Key);
            }
        }
    }
}
