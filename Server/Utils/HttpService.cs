using System.Net;
using System.Text;
using Server.MirNetwork;

namespace Server.Library.Utils
{
    internal abstract class HttpService
    {
        protected string Host;
        private readonly object _listenerLock = new();
        private HttpListener _listener;
        private volatile bool _isActive = true;

        protected HttpService()
        {
        }

        public void Listen(object obj)
        {
            CancellationToken token = (CancellationToken)obj;

            if (!HttpListener.IsSupported)
            {
                throw new InvalidOperationException(
                    "To use HttpListener the operating system must be Windows XP SP2 or Server 2003 or higher.");
            }
            string[] prefixes = { Host };

            try
            {
                lock (_listenerLock)
                {
                    if (!_isActive || token.IsCancellationRequested)
                        return;

                    _listener = new HttpListener();
                    foreach (var s in prefixes)
                    {
                        _listener.Prefixes.Add(s);
                    }
                    _listener.Start();
                }
                MessageQueue.Instance.Enqueue(GameLanguage.ServerTextMap.GetLocalization(ServerTextKeys.HttpServiceStarted));
            }
            catch (Exception err)
            {
                MessageQueue.Instance.Enqueue(GameLanguage.ServerTextMap.GetLocalization(ServerTextKeys.HttpServiceStartFailed) + err);
                lock (_listenerLock)
                    _listener?.Close();
                throw;
            }

            try
            {
                while (_isActive && !token.IsCancellationRequested)
                {
                    var context = _listener.GetContext();
                    try
                    {
                        var request = context.Request;
                        Console.WriteLine("{0} {1} HTTP/1.1", request.HttpMethod, request.RawUrl);
                        Console.WriteLine("User-Agent: {0}", request.UserAgent);
                        Console.WriteLine("Accept-Encoding: {0}", request.Headers["Accept-Encoding"]);
                        Console.WriteLine("Connection: {0}", request.KeepAlive ? "Keep-Alive" : "close");
                        Console.WriteLine("Host: {0}", request.UserHostName);
                        var response = context.Response;

                        if (context.Request.RemoteEndPoint != null)
                        {
                            // Normalised so an IPv4 client arriving over a dual-mode socket still
                            // matches a 127.0.0.1 trust entry.
                            var clientIp = NetUtil.ClientIP(context.Request.RemoteEndPoint);

                            if (!IsTrustedAddress(clientIp, Settings.HTTPTrustedIPAddress))
                            {
                                WriteResponse(response, "notrusted:" + clientIp);
                                continue;
                            }
                        }
                        {
                            var clientIp = context.Request.RemoteEndPoint.Address.ToString();

                            if (clientIp != Settings.HTTPTrustedIPAddress)
                            {
                                WriteResponse(response, "notrusted:" + clientIp);
                                continue;
                            }
                        }

                        if (request.HttpMethod == "GET")
                        {
                            OnGetRequest(request, response);
                        }
                        else
                        {
                            OnPostRequest(request, response);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception) when (!_isActive || token.IsCancellationRequested)
            {
                // Closing the listener interrupts a pending GetContext.
            }
            finally
            {
                lock (_listenerLock)
                    _listener?.Close();
            }
        }

        /// <summary>
        /// Loopback may arrive as 127.0.0.1 or ::1 depending on the address family.
        /// </summary>
        private static bool IsTrustedAddress(string clientIp, string trustedIp)
        {
            if (clientIp == trustedIp) return true;

            return (clientIp == "::1" && trustedIp == "127.0.0.1") ||
                   (clientIp == "127.0.0.1" && trustedIp == "::1");
        }

        public void Stop()
        {
            lock (_listenerLock)
            {
                _isActive = false;
                _listener?.Close();
            }
        }

        public abstract void OnGetRequest(HttpListenerRequest request, HttpListenerResponse response);
        public abstract void OnPostRequest(HttpListenerRequest request, HttpListenerResponse response);

        public void WriteResponse(HttpListenerResponse response, string responseString)
        {
            try
            {
                response.ContentLength64 = Encoding.UTF8.GetByteCount(responseString);
                response.ContentType = "text/html; charset=UTF-8";
            }
            finally
            {
                var output = response.OutputStream;
                var writer = new StreamWriter(output);
                writer.Write(responseString);
                writer.Close();
            }
        }
    }
}
