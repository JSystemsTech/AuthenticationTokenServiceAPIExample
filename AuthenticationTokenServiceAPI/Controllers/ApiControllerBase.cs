using AuthenticationTokenServiceAPI.Models;
using DbFacade.DataLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationTokenServiceAPI.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IServiceProvider Services { get; set; }
        public ApiControllerBase(IServiceProvider services)
        {
            Services = services;
        }

        protected IActionResult SuccessResponse<T>(T item)
            => Ok(APIResponse<T>.Create(item));
        protected IActionResult ErrorResponse<T>(string error, T value)
            => Ok(APIResponse<T>.Error(error, value));
        protected IActionResult ErrorResponse<T>(Exception e, T value)
            => ErrorResponse(e.Message, value);
        protected IActionResult ErrorResponse<T>(IDbResponse dbResponse, T value)
            => ErrorResponse($"{dbResponse.ErrorMessage} - {dbResponse.ErrorDetails}", value);
        protected IActionResult ErrorResponse<T, TDbDataModel>(IDbResponse<TDbDataModel> dbResponse, T value)
            where TDbDataModel : DbDataModel
            => ErrorResponse($"{dbResponse.ErrorMessage} - {dbResponse.ErrorDetails}", value);

        protected IActionResult SuccessListResponse<T>(IEnumerable<T> item)
            => SuccessResponse(item);

    }



}
