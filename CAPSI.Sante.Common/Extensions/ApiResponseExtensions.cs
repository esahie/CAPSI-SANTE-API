using CAPSI.Sante.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Common.Extensions
{
    public static class ApiResponseExtensions
    {
        public static ApiResponse<T> ToSuccessResponse<T>(this T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> ToErrorResponse<T>(this string errorMessage, IDictionary<string, string[]> validationErrors = null)
        {
            var response = new ApiResponse<T>
            {
                Success = false,
                Message = errorMessage,
                Errors = new List<string>()
            };

            if (validationErrors != null)
            {
                foreach (var error in validationErrors)
                {
                    foreach (var message in error.Value)
                    {
                        response.Errors.Add($"{error.Key}: {message}");
                    }
                }
            }

            return response;
        }
    }
}