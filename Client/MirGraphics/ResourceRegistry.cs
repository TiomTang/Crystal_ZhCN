using System.Collections;

namespace Client.MirGraphics
{
    /// <summary>Unordered, reference-identity registry with bounded incremental expiration scans.</summary>
    internal sealed class ResourceRegistry<T> : IEnumerable<T> where T : class
    {
        private readonly List<T> _items = new List<T>();
        private readonly Dictionary<T, int> _indices = new Dictionary<T, int>(ReferenceEqualityComparer.Instance);
        private readonly Func<T, long> _getExpiry;
        private readonly Action<T> _dispose;
        private readonly Func<T, long> _getBytes;
        private int _cursor;
        private int _remaining;

        public int Count => _items.Count;
        public long EstimatedBytes { get; private set; }
        public bool HasPendingSweep => _remaining > 0 && _items.Count > 0;
        public T this[int index] => _items[index];

        public ResourceRegistry(Func<T, long> getExpiry, Action<T> dispose, Func<T, long> getBytes = null)
        {
            _getExpiry = getExpiry;
            _dispose = dispose;
            _getBytes = getBytes;
        }

        public void Add(T item)
        {
            if (item == null || !_indices.TryAdd(item, _items.Count)) return;
            _items.Add(item);
            if (_getBytes != null) EstimatedBytes += _getBytes(item);
        }

        public bool Remove(T item)
        {
            if (item == null || !_indices.Remove(item, out int index)) return false;
            int last = _items.Count - 1;
            if (index != last)
            {
                T moved = _items[last];
                _items[index] = moved;
                _indices[moved] = index;
            }
            _items.RemoveAt(last);
            if (_getBytes != null) EstimatedBytes -= _getBytes(item);
            return true;
        }

        public void StartSweep()
        {
            // Finish an existing pass instead of restarting at the first item every second.
            if (!HasPendingSweep) _remaining = Count;
        }

        public bool CleanOne(long now)
        {
            if (!HasPendingSweep) return false;
            if (_cursor >= Count) _cursor = 0;
            T item = _items[_cursor];
            _remaining--;
            if (now > _getExpiry(item))
            {
                _dispose(item);
                // Dispose normally unregisters itself. Also handle already-disposed resources.
                Remove(item);
                return true;
            }
            _cursor++;
            return false;
        }

        public void Clear()
        {
            _items.Clear();
            _indices.Clear();
            EstimatedBytes = 0;
            _cursor = 0;
            _remaining = 0;
        }

        public List<T>.Enumerator GetEnumerator() => _items.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
