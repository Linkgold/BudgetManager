namespace UI.Services.Interface
{
    public interface ILogService
    {
        Task LogErrorAsync(string message, Exception? exception = null);
        Task LogWarningAsync(string message);
        Task LogInfoAsync(string message);
    }
}