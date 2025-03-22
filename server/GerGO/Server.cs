using System.Net;
using System.Net.Sockets;

using GerGO.utils;

namespace GerGO
{
    class Server
    {
        private bool _running = false;
        private TcpListener _listener;

        private Logger _logger = LoggerFactory.GetLogger();

        public Server(string IpAddress, int port)
        {
            _running = false;
            _listener = new TcpListener(IPAddress.Parse(IpAddress), port);
        }

        public void Start()
        {
            _running = true;
            _listener.Start();

            _logger.Info("Server started!");
        }

        public void Stop()
        {
            _listener.Stop();
            _logger.Info("Server stopped!");
        }

        public void Run()
        {
            while (_running)
            {
                TcpClient client = _listener.AcceptTcpClient();
                _ = Task.Run(() => new RequestHandler(client));
            }
        }
    }
}
