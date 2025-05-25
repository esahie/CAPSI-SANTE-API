using CAPSI.Sante.Application.DTOs.Auth;
using CAPSI.Sante.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<TokenDto>> LoginAsync(LoginDto model);
        Task<ApiResponse<TokenDto>> RegisterAsync(RegisterDto model);
        Task<ApiResponse<TokenDto>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse<bool>> RevokeTokenAsync(string email);
        Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto model);
        Task<ApiResponse<bool>> ValidateTokenAsync(string token);
    }
}
