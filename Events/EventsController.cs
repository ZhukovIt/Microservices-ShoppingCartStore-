using Api.ShoppingCarts;
using Api.Utils;
using CSharpFunctionalExtensions;
using MicroservicesCommon;
using MicroservicesCommon.Api;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShoppingCartLogic.Events;
using ShoppingCartLogic.ShoppingCarts;
using ShoppingCartLogic.Utils;

namespace Api.Events
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : BaseController
    {
        private ShoppingCartLogic.Events.EventStore m_EventStore;
        //------------------------------------------------------------------------------------------
        public EventsController(UnitOfWork _UnitOfWork, ShoppingCartLogic.Events.EventStore _EventStore)
            : base(_UnitOfWork)
        {
            m_EventStore = _EventStore;
        }
        //------------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAllEventsInTheRange([FromQuery] long start, [FromQuery] long end)
        {
            if (start < 0)
                return Error("Значение параметра start должно быть больше или равно нуля!");
            if (end <= 0)
                return Error("Значение параметра end должно быть больше или равно нуля!");
            if (start > end)
                return Error("Значение параметра start не может быть больше значения параметра end!");

            IReadOnlyList<ShoppingCartEvent> events = await m_EventStore.GetEvents(start, end);

            List<EventDto> dtos = events
                .Select(ev => new EventDto()
                {
                    Id = ev.Id,
                    OccuredAt = ev.OccuredAt,
                    Name = ev.Name,
                    ShoppingCartItem = new ShoppingCartItemDto(JsonConvert.DeserializeObject<SerializableShoppingCartItem>(ev.Content)!)
                })
                .ToList();

            return Ok(dtos);
        }
        //------------------------------------------------------------------------------------------
    }
}
