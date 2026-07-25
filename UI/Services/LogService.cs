using Microsoft.JSInterop;
using UI.Services.Interfaces;

namespace UI.Services
{
    public class LogService : ILogService
    {
        private readonly IJSRuntime _jsRuntime;

        public LogService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task LogErrorAsync(string message, Exception? exception = null)
        {
            string fullMessage = exception != null
                ? $"[ERROR] {message} - {exception.Message}\n{exception.StackTrace}"
                : $"[ERROR] {message}";

            Console.Error.WriteLine(fullMessage);
            await _jsRuntime.InvokeVoidAsync("console.error", fullMessage);
        }

        public async Task LogWarningAsync(string message)
        {
            Console.WriteLine($"[WARNING] {message}");
            await _jsRuntime.InvokeVoidAsync("console.warn", message);
        }

        public async Task LogInfoAsync(string message)
        {
            Console.WriteLine($"[INFO] {message}");
            await _jsRuntime.InvokeVoidAsync("console.log", message);
        }
    }
}