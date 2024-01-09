using ShoppingCartLogic.ShoppingCarts;

namespace Api.ShoppingCarts
{
    public class ShoppingCartDto
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public List<ShoppingCartItemDto> ShoppingCartItems { get; set; }

        public ShoppingCartDto(ShoppingCart shoppingCart)
        {
            Id = shoppingCart.Id;
            UserId = shoppingCart.UserId;
            ShoppingCartItems = shoppingCart.Items
                .Select(x => new ShoppingCartItemDto(x))
                .ToList();
        }
    }
}
