using Microsoft.JSInterop;
using UI.Services.Interfaces;

namespace UI.Services
{
    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;
        private readonly IJSRuntime _jsRuntime;

        public LogService(ILogger<LogService> logger, IJSRuntime jsRuntime)
        {
            _logger = logger;
            _jsRuntime = jsRuntime;
        }

        public async Task LogErrorAsync(string message, Exception? exception = null)
        {
            if (exception is not null)
            {
                _logger.LogError(exception, "{Message}", message);
            }
            else
            {
                _logger.LogError("{Message}", message);
            }

            await WriteToBrowserConsoleAsync("error", exception is null ? message : $"{message}\n{exception}");
        }

        public async Task LogWarningAsync(string message)
        {
            _logger.LogWarning("{Message}", message);
            await WriteToBrowserConsoleAsync("warn", message);
        }

        public async Task LogInformationAsync(string message)
        {
            _logger.LogInformation("{Message}", message);
            await WriteToBrowserConsoleAsync("log", message);
        }

        private async Task WriteToBrowserConsoleAsync(string logLevel, string message)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync($"console.{logLevel}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to write to browser console.");
            }
        }
    }
}