using ShoppingCartLogic.ShoppingCarts;

namespace Api.ShoppingCarts
{
    public class ShoppingCartItemDto
    {
        public MoneyDto Money { get; set; }

        public long ProductCatalogId { get; set; }

        public string ProductName { get; set; }

        public string? ProductDescription { get; set; }

        public ShoppingCartItemDto(ShoppingCartItem shoppingCartItem)
        {
            ProductCatalogId = shoppingCartItem.ProductId;
            ProductName = shoppingCartItem.Name;
            if (shoppingCartItem.Description.HasValue)
                ProductDescription = shoppingCartItem.Description.Value;
            Money = new MoneyDto()
            {
                Amount = shoppingCartItem.Money.Amount,
                Currency = shoppingCartItem.Money.Currency
            };
        }

        public ShoppingCartItemDto(SerializableShoppingCartItem item)
        {
            ProductCatalogId = item.ProductId;
            ProductName = item.Name;
            if (item.Description != null)
                ProductDescription = item.Description;
            Money = new MoneyDto()
            {
                Amount = item.Amount,
                Currency = item.Currency
            };
        }
    }
}
