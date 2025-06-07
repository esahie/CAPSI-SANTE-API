using CAPSI.Sante.Application.DTOs.Auth;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<TokenDto>> LoginAsync(LoginDto model)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(model.Email);
                if (user == null || !VerifyPasswordHash(model.Password, user.PasswordHash))
                {
                    return new ApiResponse<TokenDto>
                    {
                        Success = false,
                        Message = "Email ou mot de passe incorrect"
                    };
                }

                if (!user.IsActive)
                {
                    return new ApiResponse<TokenDto>
                    {
                        Success = false,
                        Message = "Compte désactivé"
                    };
                }

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                user.LastLogin = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                return new ApiResponse<TokenDto>
                {
                    Success = true,
                    Data = new TokenDto
                    {
                        AccessToken = token,
                        RefreshToken = refreshToken,
                        Expiration = DateTime.UtcNow.AddMinutes(60)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion pour {Email}", model.Email);
                return new ApiResponse<TokenDto>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la connexion"
                };
            }
        }

        private string GenerateJwtToken(User user)
        {
            _logger.LogInformation("Génération de token pour l'utilisateur: {Email}, ID: {Id}, Role: {Role}",
       user.Email, user.UserId, user.UserType);

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            // Ajouter une vérification pour chaque valeur potentiellement null
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Role, user.UserType ?? ""),
        new Claim(ClaimTypes.NameIdentifier, user.Email ?? "")
    };

            // Ajouter l'ID utilisateur seulement s'il n'est pas null ou vide
            if (user.UserId != Guid.Empty && user.UserId != null)
            {
                claims.Add(new Claim("uid", user.UserId.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims.ToArray(),
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

       
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<ApiResponse<TokenDto>> RegisterAsync(RegisterDto model)
        {
            try
            {
                // Vérifier si l'email existe déjà
                var existingUser = await _userRepository.GetByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return new ApiResponse<TokenDto>
                    {
                        Success = false,
                        Message = "Cet email est déjà utilisé"
                    };
                }

                // Valider le rôle
                if (!IsValidRole(model.Role))
                {
                    return new ApiResponse<TokenDto>
                    {
                        Success = false,
                        Message = "Rôle invalide"
                    };
                }

                // Créer le nouveau utilisateur
                var user = new User
                {
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    UserType = model.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);

                // Générer les tokens
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userRepository.UpdateAsync(user);

                return new ApiResponse<TokenDto>
                {
                    Success = true,
                    Data = new TokenDto
                    {
                        AccessToken = token,
                        RefreshToken = refreshToken,
                        Expiration = DateTime.UtcNow.AddMinutes(60)
                    },
                    Message = "Inscription réussie"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription pour {Email}", model.Email);
                return new ApiResponse<TokenDto>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de l'inscription"
                };
            }
        }

        public async Task<ApiResponse<TokenDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
                if (user == null ||
                    user.RefreshToken != refreshToken ||
                    user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return new ApiResponse<TokenDto>
                    {
                        Success = false,
                        Message = "Token de rafraîchissement invalide ou expiré"
                    };
                }

                var newAccessToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userRepository.UpdateAsync(user);

                return new ApiResponse<TokenDto>
                {
                    Success = true,
                    Data = new TokenDto
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,
                        Expiration = DateTime.UtcNow.AddMinutes(60)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
                return new ApiResponse<TokenDto>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors du rafraîchissement du token"
                };
            }
        }

        public async Task<ApiResponse<bool>> RevokeTokenAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    };
                }

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userRepository.UpdateAsync(user);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Token révoqué avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la révocation du token pour {Email}", email);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la révocation du token"
                };
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto model)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(model.Email);
                if (user == null || !VerifyPasswordHash(model.CurrentPassword, user.PasswordHash))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Email ou mot de passe actuel incorrect"
                    };
                }

                user.PasswordHash = HashPassword(model.NewPassword);
                await _userRepository.UpdateAsync(user);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Mot de passe modifié avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de mot de passe pour {Email}", model.Email);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors du changement de mot de passe"
                };
            }
        }

        public async Task<ApiResponse<bool>> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Token valide"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token invalide");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Token invalide"
                };
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private bool IsValidRole(string role)
        {
            return new[] { "Admin", "Medecin", "Patient" }.Contains(role);
        }
    }
}
