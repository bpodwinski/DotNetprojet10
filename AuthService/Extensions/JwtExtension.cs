using AuthService.Domain;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace AuthService.Extensions
{
    public static class JwtExtension
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            IdentityModelEventSource.ShowPII = true;

            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("JWT configuration is incomplete. Ensure JWT_ISSUER, JWT_AUDIENCE, and JWT_SECRET_KEY are set in environment variables.");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            try
                            {
                                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<UserDomain>>();
                                var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();

                                logger.LogInformation("Token validation started.");

                                var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                                if (string.IsNullOrEmpty(userId))
                                {
                                    logger.LogWarning("Token validation failed: User ID is missing.");
                                    context.Fail("Invalid token: User ID is missing.");
                                    return;
                                }

                                var user = await userManager.FindByIdAsync(userId);
                                if (user == null)
                                {
                                    logger.LogWarning("Token validation failed: User not found for ID {UserId}.", userId);
                                    context.Fail("Invalid token: User not found.");
                                    return;
                                }

                                if (context.SecurityToken is JsonWebToken token)
                                {
                                    logger.LogInformation("Checking if token is revoked.");
                                    var isRevoked = await jwtService.IsTokenRevoked(user, token.UnsafeToString());
                                    if (isRevoked)
                                    {
                                        logger.LogWarning("Token validation failed: Token has been revoked.");
                                        context.Fail("Token has been revoked.");
                                        return;
                                    }
                                }
                                else
                                {
                                    logger.LogWarning("Token validation failed: SecurityToken is not a JwtSecurityToken.");
                                    context.Fail("Invalid token: SecurityToken is invalid.");
                                    return;
                                }

                                logger.LogInformation("Token validation succeeded.");
                            }
                            catch (Exception ex)
                            {
                                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                                logger.LogError(ex, "An error occurred during token validation.");
                                context.Fail($"An error occurred during token validation: {ex.Message}");
                            }
                        }
                    };
                });
            return services;
        }
    }
}
