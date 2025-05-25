
using CAPSI.Sante.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAPSI.Sante.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected ActionResult<ApiResponse<T>> HandleResponse<T>(ApiResponse<T> response)
        {
            if (!response.Success)
                return response.Errors?.Any() == true
                    ? BadRequest(response)
                    : NotFound(response);

            return Ok(response);
        }
    }
}
