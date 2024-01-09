using Api.OtherMicroservicesClients;
using Api.Utils;
using CSharpFunctionalExtensions;
using MicroservicesCommon;
using Microsoft.AspNetCore.Mvc;
using ShoppingCartLogic.Events;
using ShoppingCartLogic.ShoppingCarts;
using ShoppingCartLogic.Utils;

namespace Api.ShoppingCarts
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : BaseController
    {
        private readonly ShoppingCartStore _ShoppingCartStore;
        private readonly ShoppingCartItemStore _ShoppingCartItemStore;
        private readonly ProductCatalogClient _ProductCatalog;
        private readonly ShoppingCartLogic.Events.EventStore _EventStore;
        //--------------------------------------------------------------------------------------
        public ShoppingCartController(
            UnitOfWork _UnitOfWork,
            ProductCatalogClient _ProductCatalog,
            ShoppingCartStore _ShoppingCartStore,
            ShoppingCartItemStore _ShoppingCartItemStore,
            ShoppingCartLogic.Events.EventStore _EventStore) : base(_UnitOfWork)
        {
            this._ShoppingCartStore = _ShoppingCartStore;
            this._ShoppingCartItemStore = _ShoppingCartItemStore;
            this._ProductCatalog = _ProductCatalog;
            this._EventStore = _EventStore;
        }
        //--------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult DefaultRoute()
        {
            return Ok("Микросервис ShoppingCart работает!");
        }
        //--------------------------------------------------------------------------------------
        [HttpGet]
        [Route("{userid:int}")]
        public async Task<IActionResult> GetShoppingCartByUserId(int userid)
        {
            ShoppingCart shoppingCart = await _ShoppingCartStore.GetByUserId(userid).ConfigureAwait(false);
            if (shoppingCart == null)
                return NotFound($"Корзина покупок для пользователя с Id = {userid} отсутствует!");

            ShoppingCartDto dto = new ShoppingCartDto(shoppingCart);

            return Ok(dto);
        }
        //--------------------------------------------------------------------------------------
        [HttpPost]
        [Route("{userid:int}")]
        public async Task<IActionResult> AddNewShoppingCartByUserId(int userid)
        {
            ShoppingCart shoppingCart = await _ShoppingCartStore.GetByUserId(userid).ConfigureAwait(false);
            if (shoppingCart != null)
                return Error($"Корзина покупок для пользователя с Id = {userid} уже существует!");

            shoppingCart = new ShoppingCart(userid);
            _ShoppingCartStore.Add(shoppingCart);

            ShoppingCartDto dto = new ShoppingCartDto(shoppingCart);

            return Ok(dto);
        }
        //--------------------------------------------------------------------------------------
        [HttpPost]
        [Route("{userid:int}/items")]
        public async Task<IActionResult> AddNewProductsInUserShoppingCart(int userid, [FromBody] AddNewProductsDto items)
        {
            ShoppingCart shoppingCart = await _ShoppingCartStore.GetByUserId(userid).ConfigureAwait(false);
            if (shoppingCart == null)
                return NotFound($"Корзина покупок для пользователя с Id = {userid} отсутствует!");

            Result<IEnumerable<ShoppingCartItem>> shoppingCartItemsOrError = await _ProductCatalog
                .GetShoppingCartItemsAsync(items.ProductCatalogIds)
                .ConfigureAwait(false);
            if (shoppingCartItemsOrError.IsFailure)
                return NotFound(shoppingCartItemsOrError.Error);

            shoppingCart.AddItems(shoppingCartItemsOrError.Value);
            _ShoppingCartItemStore.Add(shoppingCart.Items);
            _ShoppingCartStore.Add(shoppingCart);

            ShoppingCartDto dto = new ShoppingCartDto(shoppingCart);

            foreach (IDomainEvent domainEvent in shoppingCart.DomainEvents)
            {
                DomainEvents.Dispatch(domainEvent, _EventStore);
            }

            return Ok(dto);
        }
        //--------------------------------------------------------------------------------------
        [HttpDelete]
        [Route("{userid:int}/items")]
        public async Task<IActionResult> DeleteProductsInUserShoppingCart(int userid, [FromBody] DeleteProductsDto items)
        {
            ShoppingCart shoppingCart = await _ShoppingCartStore.GetByUserId(userid).ConfigureAwait(false);
            if (shoppingCart == null)
                return NotFound($"Корзина покупок для пользователя с Id = {userid} отсутствует!");

            IEnumerable<ShoppingCartItem> deletedProducts = shoppingCart.Items
                .Where(item => items.ProductCatalogIds.Contains(item.ProductId));
            shoppingCart.RemoveItems(deletedProducts);
            _ShoppingCartItemStore.Delete(deletedProducts);
            _ShoppingCartStore.Add(shoppingCart);

            ShoppingCartDto dto = new ShoppingCartDto(shoppingCart);

            foreach (IDomainEvent domainEvent in shoppingCart.DomainEvents)
            {
                DomainEvents.Dispatch(domainEvent, _EventStore);
            }

            return Ok(dto);
        }
        //--------------------------------------------------------------------------------------
    }
}
