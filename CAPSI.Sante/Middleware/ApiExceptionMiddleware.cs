
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Exceptions;

namespace CAPSI.Sante.API.Middleware
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur non gérée est survenue");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                ValidationException _ => StatusCodes.Status400BadRequest,
                NotFoundException _ => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = ex is ValidationException ? ex.Message : "Une erreur est survenue",
                Errors = ex is ValidationException ? new List<string> { ex.Message } : null
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
