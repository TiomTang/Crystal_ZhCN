using System.Net;
using System.Net.Sockets;

namespace Client.MirNetwork
{
    /// <summary>
    /// Turns the configured host into an ordered list of addresses and tries them one at a
    /// time. IPv6 goes first, but every attempt is bounded: an address that silently drops
    /// packets would otherwise hold the client on the system connect timeout before the IPv4
    /// fallback is even tried.
    /// </summary>
    public static class ConnectionCandidates
    {
        /// <summary>
        /// Budget for one address while another address is still waiting behind it.
        /// </summary>
        public const int AttemptTimeout = 250;

        /// <summary>
        /// Budget for the last address. Nothing follows it, so cutting it short would only
        /// turn a slow but working connection into a failure.
        /// </summary>
        public const int FinalAttemptTimeout = 3000;

        /// <summary>
        /// IPv6 first, IPv4 after, keeping the resolver's order inside each family.
        /// </summary>
        public static IPAddress[] Order(IEnumerable<IPAddress> addresses)
        {
            if (addresses == null) return Array.Empty<IPAddress>();

            return addresses
                .Where(address => address != null)
                .OrderBy(address => address.AddressFamily == AddressFamily.InterNetworkV6 ? 0 : 1)
                .ToArray();
        }

        /// <summary>
        /// Resolves the configured host. Literal addresses are used as given, names go to DNS;
        /// an empty result means the name did not resolve this time.
        /// </summary>
        public static IPAddress[] Resolve(string host)
        {
            if (string.IsNullOrWhiteSpace(host)) return Array.Empty<IPAddress>();

            host = host.Trim();

            if (IPAddress.TryParse(host, out IPAddress literal))
                return new[] { literal };

            try
            {
                return Order(Dns.GetHostAddresses(host));
            }
            catch (SocketException)
            {
                return Array.Empty<IPAddress>();
            }
            catch (ArgumentException)
            {
                return Array.Empty<IPAddress>();
            }
        }

        /// <summary>
        /// Connects to the first address that answers, waiting at most
        /// <paramref name="attemptTimeout"/> per address while others remain, and
        /// <paramref name="finalAttemptTimeout"/> for the last one.
        /// </summary>
        public static async Task<Socket> ConnectAsync(IPAddress[] addresses, int port,
                                                      int attemptTimeout, int finalAttemptTimeout,
                                                      CancellationToken cancellationToken)
        {
            if (addresses == null || addresses.Length == 0)
                throw new ArgumentException("no addresses to try", nameof(addresses));

            var failures = new List<Exception>();

            for (int i = 0; i < addresses.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int timeout = i == addresses.Length - 1 ? finalAttemptTimeout : attemptTimeout;

                try
                {
                    return await TryConnectAsync(addresses[i], port, timeout, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
            }

            throw new AggregateException($"none of {addresses.Length} address(es) accepted the connection", failures);
        }

        /// <summary>
        /// Marks a task as observed so an abandoned connect does not surface as an unobserved
        /// task exception later.
        /// </summary>
        private static void Forget(Task task)
        {
            task.ContinueWith(finished => { _ = finished.Exception; },
                              CancellationToken.None,
                              TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                              TaskScheduler.Default);
        }

        private static async Task<Socket> TryConnectAsync(IPAddress address, int port, int timeout,
                                                          CancellationToken cancellationToken)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            bool connected = false;

            try
            {
                using var attempt = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                Task connect = socket.ConnectAsync(new IPEndPoint(address, port), attempt.Token).AsTask();
                Task timeoutTask = Task.Delay(timeout, cancellationToken);

                if (await Task.WhenAny(connect, timeoutTask).ConfigureAwait(false) != connect)
                {
                    // Abort by closing the socket: waiting for the cancelled connect to settle
                    // can take as long as the system timeout we are trying to avoid.
                    attempt.Cancel();
                    socket.Dispose();
                    Forget(connect);

                    cancellationToken.ThrowIfCancellationRequested();

                    throw new TimeoutException($"{address} did not answer within {timeout}ms");
                }

                await connect.ConfigureAwait(false);

                connected = true;
                return socket;
            }
            finally
            {
                if (!connected) socket.Dispose();
            }
        }
    }
}
