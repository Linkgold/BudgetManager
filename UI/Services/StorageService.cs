using Microsoft.JSInterop;
using UI.Services.Interfaces;

namespace UI.Services
{
    public class StorageService : IStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly string _storageType;

        public StorageService(IJSRuntime jsRuntime, IConfiguration configuration)
        {
            _jsRuntime = jsRuntime;
            _storageType = configuration["StorageType"] ?? "Session";

            if (_storageType != "Session" && _storageType != "Local")
            {
                _storageType = "Session"; // Default to Session if the configuration value is invalid
            }
        }

        public async Task SetItemAsync(string key, string value) => await _jsRuntime.InvokeVoidAsync($"{_storageType.ToLowerInvariant()}Storage.setItem", key, value);

        public async Task<string?> GetItemAsync(string key) => await _jsRuntime.InvokeAsync<string>($"{_storageType.ToLowerInvariant()}Storage.getItem", key);

        public async Task RemoveItemAsync(string key) => await _jsRuntime.InvokeVoidAsync($"{_storageType.ToLowerInvariant()}Storage.removeItem", key);
    }
}