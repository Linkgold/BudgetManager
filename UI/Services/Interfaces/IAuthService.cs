using Shared.DTOs.Request;

namespace UI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginRequestDTO request);
        Task<bool> RegisterAsync(CreateUserRequestDTO request);
        Task LogoutAsync();
        Task<string?> GetTokenAsync();
        bool IsAuthenticated { get; }
    }
}