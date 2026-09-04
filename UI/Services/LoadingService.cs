namespace UI.Services
{
    public class LoadingService
    {
        public event Action? OnLoadingChanged;
        private int _pendingRequests = 0;

        public bool IsLoading => _pendingRequests > 0;

        public void Show()
        {
            _pendingRequests++;

            NotifyStateChanged();
        }

        public void Hide()
        {
            if (_pendingRequests > 0)
                _pendingRequests--;

            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnLoadingChanged?.Invoke();
    }
}