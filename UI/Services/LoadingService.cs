namespace UI.Services
{
    public class LoadingService : IDisposable
    {
        public event Action? OnLoadingChanged;
        public event Action<string>? OnLoadingTextChanged;

        private CancellationTokenSource? _cts;
        private int _pendingRequests = 0;
        private int _messageIndex = 0;

        private readonly string[] _messages = new[]
        {
            "Cargando datos...",
            "⏳ Procesando la solicitud, gracias por su paciencia...",
            "⏳ La API está iniciándose, espere unos instantes...",
            "⏳ La operación puede demorarse unos segundos..."
        };

        public bool IsLoading => _pendingRequests > 0;
        public string CurrentMessage { get; private set; } = "Cargando datos...";

        public void Show()
        {
            _pendingRequests++;
            CurrentMessage = _messages[0];
            _messageIndex = 0;

            NotifyStateChanged();
            StartTimer();
        }

        public void Hide()
        {
            if (_pendingRequests > 0)
                _pendingRequests--;

            if (_pendingRequests == 0)
            {
                StopTimer();
                CurrentMessage = "Cargando datos...";
                _messageIndex = 0;
                NotifyStateChanged();
            }

            NotifyStateChanged();
        }

        private void StartTimer()
        {
            StopTimer();
            _cts = new CancellationTokenSource();
            _ = RunPeriodicUpdate(_cts.Token);
        }

        private void StopTimer()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private async Task RunPeriodicUpdate(CancellationToken token)
        {
            try
            {
                using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(12));

                while (await timer.WaitForNextTickAsync(token))
                {
                    _messageIndex = (_messageIndex + 1) % _messages.Length;
                    CurrentMessage = _messages[_messageIndex];
                    OnLoadingTextChanged?.Invoke(CurrentMessage);
                }
            }
            catch (OperationCanceledException)
            {
                // Esperado al cancelar
            }
        }

        private void NotifyStateChanged() => OnLoadingChanged?.Invoke();

        public void Dispose()
        {
            StopTimer();
        }
    }
}