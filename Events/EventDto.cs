using Api.ShoppingCarts;

namespace Api.Events
{
    public class EventDto
    {
        public long Id { get; set; }

        public DateTime OccuredAt { get; set; }

        public string Name { get; set; }

        public ShoppingCartItemDto ShoppingCartItem { get; set; }
    }
}
