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
            try
            {
                string fullMessage = exception != null
                ? $"[ERROR] {message} - {exception.Message}\n{exception.StackTrace}"
                : $"[ERROR] {message}";

                Console.WriteLine(fullMessage);

                // ✅ Manejar posible error de JS
                try
                {
                    await _jsRuntime.InvokeVoidAsync("console.error", fullMessage);
                }
                catch
                {
                    // ✅ Si falla JS, solo log en consola
                    Console.Error.WriteLine("JS console.error failed, but log was written to console.");
                }
            }
            catch
            {
                // ✅ Si todo falla, al menos escribir en consola
                Console.Error.WriteLine($"FATAL: Could not log error: {message}");
            }
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