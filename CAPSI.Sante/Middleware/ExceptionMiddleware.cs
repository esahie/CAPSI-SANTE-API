
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Exceptions;
using Serilog.Context;

namespace CAPSI.Sante.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur inattendue s'est produite.");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var requestId = context.TraceIdentifier;

            // Enrichir le log avec des informations sur la requête
            using (LogContext.PushProperty("RequestId", requestId))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            using (LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress))
            {
                _logger.LogError(exception, "Une erreur inattendue s'est produite.");
            }

            context.Response.ContentType = "application/json";
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Une erreur s'est produite lors du traitement de la requête.",
                RequestId = requestId
            };

            switch (exception)
            {
                case ValidationException validationEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Erreur de validation";
                    response.Errors = (List<string>)validationEx.Errors;
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = "Non autorisé";
                    break;

                case NotFoundException notFoundEx:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = notFoundEx.Message;
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
