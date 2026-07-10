namespace Application.Interfaces
{
    /// <summary>
    /// Servicio para obtener información del usuario autenticado
    /// </summary>
    public interface ICurrentUserService
    {
        int UserId { get; }
        bool IsAuthenticated { get; }
        string? UserName { get; }
        string? Email { get; }
    }
}