namespace UI.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<string?> GetTokenAsync();
        bool IsAuthenticated { get; }
        string? UserName { get; }
        string? Email { get; }
    }
}
