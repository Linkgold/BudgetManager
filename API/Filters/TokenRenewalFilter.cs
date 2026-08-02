using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Filters
{
    public class TokenRenewalFilter : IAsyncActionFilter
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenRenewalFilter> _logger;

        public TokenRenewalFilter(IConfiguration configuration, ILogger<TokenRenewalFilter> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // ✅ Registrar el callback ANTES de ejecutar la acción
            context.HttpContext.Response.OnStarting(() =>
            {
                // ✅ Cuando la respuesta esté a punto de enviarse, añadir el header
                if (context.HttpContext.User.Identity?.IsAuthenticated == true &&
                    context.HttpContext.Response.StatusCode == 200)
                {
                    try
                    {
                        string? newToken = GenerateNewToken(context.HttpContext.User);
                        if (!string.IsNullOrEmpty(newToken))
                        {
                            context.HttpContext.Response.Headers.Append("X-New-Token", newToken);
                            _logger.LogInformation("Token renovado para usuario: {User}", context.HttpContext.User.Identity.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al renovar el token");
                    }
                }
                return Task.CompletedTask;
            });

            // ✅ Ejecutar la acción
            await next();
        }

        private string? GenerateNewToken(ClaimsPrincipal user)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            string? key = _configuration["Jwt:Key"];
            string? issuer = _configuration["Jwt:Issuer"];
            string? audience = _configuration["Jwt:Audience"];
            string? expirationString = _configuration["Jwt:ExpirationInMinutes"];

            if 
            (
                string.IsNullOrEmpty(key) || 
                string.IsNullOrEmpty(issuer) ||
                string.IsNullOrEmpty(audience) || 
                string.IsNullOrEmpty(expirationString)
            )
            {
                return null;
            }

            int expirationInMinutes = int.Parse(expirationString);

            // ✅ Calcular el umbral de renovación (80%)
            double thresholdMinutes = expirationInMinutes * 0.8;
            DateTime thresholdTime = DateTime.UtcNow.AddMinutes(thresholdMinutes);

            // ✅ Obtener el tiempo de expiración actual del token
            DateTime? currentExpiry = user.FindFirst("exp")?.Value != null
                ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(user.FindFirst("exp")!.Value)).UtcDateTime
                : (DateTime?)null;

            // ✅ Si el token actual NO ha superado el umbral del 80%, NO renovar
            if (currentExpiry.HasValue && currentExpiry.Value > thresholdTime)
            {
                return null;
            }

            _logger.LogInformation("🔄 Renovando token");

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // ✅ Obtener claims del usuario actual
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""),
                new Claim(ClaimTypes.Name, user.Identity?.Name ?? ""),
                new Claim(ClaimTypes.Email, user.FindFirst(ClaimTypes.Email)?.Value ?? "")
            };

            DateTime expires = DateTime.UtcNow.AddMinutes(expirationInMinutes);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return tokenHandler.WriteToken(token);
        }
    }
}
