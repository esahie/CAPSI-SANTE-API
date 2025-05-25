
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Exceptions;

namespace CAPSI.Sante.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

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

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = _env.IsDevelopment() ? ex.Message : "Une erreur interne est survenue"
            };

            switch (ex)
            {
                case ValidationException _:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = ex.Message;
                    break;

                case NotFoundException _:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = ex.Message;
                    break;

                case UnauthorizedAccessException _:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = "Non autorisé";
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    if (_env.IsDevelopment())
                    {
                        response.Errors = new List<string>
                    {
                        ex.StackTrace ?? "No stack trace available"
                    };
                    }
                    break;
            }

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
