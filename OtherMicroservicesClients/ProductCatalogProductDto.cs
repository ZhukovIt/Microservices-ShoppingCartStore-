using Api.ShoppingCarts;
using ProductCatalogLogic.ProductCatalog;
using System.ComponentModel.DataAnnotations;

namespace Api.OtherMicroservicesClients
{
    public class ProductCatalogProductDto
    {
        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public string? ProductDescription { get; set; }

        public MoneyDto Price { get; set; }

        public ProductCatalogProductDto() { }

        public ProductCatalogProductDto(Product _Product) : this()
        {
            ProductId = _Product.Id;
            ProductName = _Product.Name;
            if (_Product.Description.HasValue)
                ProductDescription = _Product.Description.Value;
            Price = new MoneyDto()
            {
                Amount = _Product.Money.Amount,
                Currency = _Product.Money.Currency
            };
        }
    }
}
