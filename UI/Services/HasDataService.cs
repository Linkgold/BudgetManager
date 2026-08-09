using UI.Extensions;
using UI.Models;
using UI.Services.API;
using UI.Services.Interfaces;

namespace UI.Services
{
    public class HasDataService
    {
        private readonly APIService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogService _logService;
        private readonly HasDataCache _cache;

        public HasDataService(APIService apiService, IAuthService authService, ILogService logService)
        {
            _apiService = apiService;
            _authService = authService;
            _logService = logService;
            _cache = new HasDataCache();
        }

        public async Task<HasDataModel> GetDataAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && _cache.TryGet(out HasDataModel? cachedData))
            {
                return cachedData!;
            }

            try
            {
                HasDataModel? data = await _apiService.GetHasDataAsync();
                HasDataModel result = data ?? new HasDataModel();

                _cache.Set(result);

                await _logService.LogInformationAsync("HasData cache actualizado.");

                return result;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Error al cargar HasData", ex);

                if (_cache.TryGet(out HasDataModel? cached))
                {
                    return cached!;
                }

                return new HasDataModel();
            }
        }

        public async Task RefreshAsync()
        {
            _cache.Clear();
            await GetDataAsync(true);
        }

        private async Task<T> ExecuteWithDataAsync<T>(Func<HasDataModel, T> action)
        {
            HasDataModel data = await GetDataAsync();
            return action(data);
        }

        // ================================================================
        // CLASE INTERNA PRIVADA PARA GESTIÓN DE CACHÉ
        // ================================================================

        private class HasDataCache
        {
            private HasDataModel? _cachedData;
            private DateTime _lastLoad = DateTime.MinValue;
            private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

            public bool TryGet(out HasDataModel? data)
            {
                if (_cachedData != null && DateTime.UtcNow - _lastLoad < _cacheDuration)
                {
                    data = _cachedData;
                    return true;
                }

                data = null;
                return false;
            }

            public void Set(HasDataModel data)
            {
                _cachedData = data;
                _lastLoad = DateTime.UtcNow;
            }

            public void Clear()
            {
                _cachedData = null;
                _lastLoad = DateTime.MinValue;
            }
        }
    }
}