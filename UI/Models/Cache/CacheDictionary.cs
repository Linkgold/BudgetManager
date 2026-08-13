namespace UI.Models.Cache
{
    public class CacheDictionary<TKey, T> where TKey : notnull
    {
        private readonly Dictionary<TKey, CacheEntry> _entries = new();

        public bool TryGetValue(TKey key, out List<T>? entities)
        {
            if (_entries.TryGetValue(key, out CacheEntry? entry))
            {
                entities = entry.Entities;
                return true;
            }

            entities = null;
            return false;
        }

        public void Add(TKey key, List<T> entities)
        {
            _entries[key] = new CacheEntry(entities, () => Remove(key));
        }

        public bool Remove(TKey key)
        {
            if (!_entries.TryGetValue(key, out CacheEntry? entry))
            {
                return false;
            }

            entry.Dispose();

            return _entries.Remove(key);
        }

        private class CacheEntry : IDisposable
        {
            public List<T> Entities { get; }

            private readonly Timer? _timer;

            public CacheEntry(List<T> entities, Action onExpired)
            {
                Entities = entities;

                _timer = new Timer
                (
                    _ =>
                    {
                        onExpired();
                        Dispose();
                    },
                    null,
                    TimeSpan.FromMinutes(5),
                    Timeout.InfiniteTimeSpan
                );
            }

            public void Dispose()
            {
                _timer?.Dispose();
            }
        }
    }
}