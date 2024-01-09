using MicroservicesCommon;
using MicroservicesCommon.Api;
using Microsoft.AspNetCore.Mvc;
using ShoppingCartLogic.Utils;

namespace Api.Utils
{
    [ApiController]
    public class BaseController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public BaseController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected new IActionResult Ok()
        {
            _unitOfWork.Commit();
            return base.Ok(Envelope.Ok());
        }

        protected IActionResult Ok<T>(T result)
        {
            _unitOfWork.Commit();
            return base.Ok(Envelope.Ok(result));
        }

        protected new IActionResult NoContent()
        {
            _unitOfWork.Commit();
            return base.NoContent();
        }

        protected IActionResult Error(string errorMessage)
        {
            return BadRequest(Envelope.Error(errorMessage));
        }

        protected IActionResult NotFound(string errorMessage)
        {
            return NotFound(Envelope.Error(errorMessage));
        }
    }
}
