using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Tests.API.Fixtures
{
    public class FakeUserMiddleware
    {
        private readonly RequestDelegate _next;

        public FakeUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Crear un usuario falso
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Email, "test@email.com")
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "Test");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            context.User = principal;

            await _next(context);
        }
    }
}