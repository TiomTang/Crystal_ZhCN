// Connection admission hardening, modified 2026-11-08.
namespace Server.MirNetwork
{
    internal sealed class ConnectionAdmission
    {
        private readonly object _gate = new object();
        private readonly Dictionary<string, int> _perIP = new Dictionary<string, int>();
        private long _windowStart, _reportStart;
        private int _windowCount, _peak;
        private long _connections, _handshakes, _timeouts, _rejected;

        public void Reset(long now)
        {
            lock (_gate)
            {
                _perIP.Clear();
                _windowStart = _reportStart = now;
                _windowCount = _peak = 0;
                _connections = _handshakes = _timeouts = _rejected = 0;
            }
        }

        public bool TryAccept(string key, long now)
        {
            lock (_gate)
            {
                _connections++;
                if (now - _windowStart >= Settings.NewConnectionWindow)
                {
                    _windowStart = now;
                    _windowCount = 0;
                    _perIP.Clear();
                }
                // Count attempts, including refusals. The global cap also bounds IP state.
                if (_windowCount >= Settings.MaxNewConnections) { _rejected++; return false; }
                _windowCount++;
                _perIP.TryGetValue(key, out int count);
                _perIP[key] = count + 1;
                if (count >= Settings.MaxNewConnectionsPerIP) { _rejected++; return false; }
                return true;
            }
        }

        public void Reject() { lock (_gate) _rejected++; }
        public void Handshake() { lock (_gate) _handshakes++; }
        public void Timeout() { lock (_gate) _timeouts++; }
        public void Observe(int pending) { lock (_gate) _peak = Math.Max(_peak, pending); }

        public string Report(long now, int pending, int authenticated)
        {
            lock (_gate)
            {
                _peak = Math.Max(_peak, pending);
                if (now - _reportStart < 900000) return null;
                string report = GameLanguage.Get("Connection admission: connections={0}, handshakes={1}, timeouts={2}, rejected={3}, unauthenticated={4}, peak={5}, authenticated={6}.",
                    _connections, _handshakes, _timeouts, _rejected, pending, _peak, authenticated);
                _reportStart = now;
                _connections = _handshakes = _timeouts = _rejected = 0;
                _peak = pending;
                return report;
            }
        }
    }
}
