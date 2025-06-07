using CAPSI.Sante.API.ModelsReponse;
using CAPSI.Sante.Application.DTOs.Auth;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CAPSI.Sante.API.Controllers.SQLServer
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginDto model)
        {
            try
            {
                var response = await _authService.LoginAsync(model);

                var loginResponse = new LoginResponse
                {
                    Success = response.Success,
                    Message = response.Message,
                    Errors = response.Errors,
                    RequestId = response.RequestId,
                    Data = response.Data != null
                        ? new TokenResponseModel
                        {
                            AccessToken = response.Data.AccessToken,
                            RefreshToken = response.Data.RefreshToken,
                            Expiration = response.Data.Expiration
                        }
                        : null
                };

                return response.Success ? Ok(loginResponse) : BadRequest(loginResponse);
            }
            catch (ValidationException ex)
            {
                var loginResponse = new LoginResponse
                {
                    Success = false,
                    Message = "Erreur de validation",
                    //Errors = ex.Errors?.SelectMany(e => e.Value).ToList() ?? new List<string>()
                };
                return BadRequest(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion");
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la connexion"
                });
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterDto model)
        {
            try
            {
                var response = await _authService.RegisterAsync(model);

                var loginResponse = new LoginResponse
                {
                    Success = response.Success,
                    Message = response.Message,
                    Errors = response.Errors,
                    RequestId = response.RequestId,
                    Data = response.Data != null
                        ? new TokenResponseModel
                        {
                            AccessToken = response.Data.AccessToken,
                            RefreshToken = response.Data.RefreshToken,
                            Expiration = response.Data.Expiration
                        }
                        : null
                };

                return response.Success
                    ? CreatedAtAction(nameof(Login), loginResponse)
                    : BadRequest(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription");
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de l'inscription"
                });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(BooleanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BooleanResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BooleanResponse>> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                model.Email = email; // Sécurité supplémentaire

                _logger.LogInformation("Token reçu - utilisateur identifié : {Email}", email ?? "NULL");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Le token JWT n’a pas fourni d’identifiant utilisateur (ClaimTypes.NameIdentifier)");
                    return Unauthorized(new BooleanResponse
                    {
                        Success = false,
                        Message = "Utilisateur non authentifié"
                    });
                }

                var response = await _authService.ChangePasswordAsync(model);

                var booleanResponse = new BooleanResponse
                {
                    Success = response.Success,
                    Message = response.Message,
                    Errors = response.Errors,
                    RequestId = response.RequestId,
                    Data = response.Data
                };

                return response.Success ? Ok(booleanResponse) : BadRequest(booleanResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de mot de passe");
                return BadRequest(new BooleanResponse
                {
                    Success = false,
                    Message = "Une erreur est survenue lors du changement de mot de passe"
                });
            }
        }

        [HttpPost("validate-token")]
        [ProducesResponseType(typeof(BooleanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BooleanResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BooleanResponse>> ValidateToken([FromBody] string token)
        {
            try
            {
                var response = await _authService.ValidateTokenAsync(token);

                var booleanResponse = new BooleanResponse
                {
                    Success = response.Success,
                    Message = response.Message,
                    Errors = response.Errors,
                    RequestId = response.RequestId,
                    Data = response.Data
                };

                return response.Success ? Ok(booleanResponse) : BadRequest(booleanResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du token");
                return BadRequest(new BooleanResponse
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la validation du token"
                });
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(refreshToken);

                var loginResponse = new LoginResponse
                {
                    Success = response.Success,
                    Message = response.Message,
                    Errors = response.Errors,
                    RequestId = response.RequestId,
                    Data = response.Data != null
                        ? new TokenResponseModel
                        {
                            AccessToken = response.Data.AccessToken,
                            RefreshToken = response.Data.RefreshToken,
                            Expiration = response.Data.Expiration
                        }
                        : null
                };

                return response.Success ? Ok(loginResponse) : BadRequest(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Une erreur est survenue lors du rafraîchissement du token"
                });
            }
        }

        [Authorize]
        [HttpPost("revoke")]
        [ProducesResponseType(typeof(BooleanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BooleanResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BooleanResponse>> RevokeToken()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new BooleanResponse
                    {
                        Success = false,
                        Message = "Email non trouvé dans le token"
                    });
                }

                var response = await _authService.RevokeTokenAsync(email);

                var booleanResponse = new BooleanResponse
                {
                    Success = response.Success,
                    Message = response.Message,
                    Errors = response.Errors,
                    RequestId = response.RequestId,
                    Data = response.Data
                };

                return response.Success ? Ok(booleanResponse) : BadRequest(booleanResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la révocation du token");
                return BadRequest(new BooleanResponse
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la révocation du token"
                });
            }
        }
    }
}