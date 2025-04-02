using System.Net;
using System.Net.Sockets;

using GerGO.Utils;

namespace GerGO
{
    static class Server
    {
        private static bool s_running = false;
        private static TcpListener s_listener;

        private static ILogger s_logger = LoggerFactory.GetLogger();

        public static void InitializeServer(string IpAddress, int port)
        {
            s_running = false;
            s_listener = new TcpListener(IPAddress.Parse(IpAddress), port);
        }

        public static void Start()
        {
            s_running = true;
            s_listener.Start();
            s_logger.Info("Server started!");
        }

        public static void Stop()
        {
            s_running = false;
            s_listener.Stop();
            s_logger.Info("Server stopped!");
        }

        public static void Run()
        {
            while (s_running)
            {
                try
                {
                    TcpClient client = s_listener.AcceptTcpClient();
                    s_logger.Info("Connected: " + client.Client.RemoteEndPoint);
                    _ = Task.Run(() => new RequestHandler(client));
                }
                catch (SocketException)
                {
                    s_logger.Warning("An operation was interrupted during stopping server.");
                }
            }
        }
    }
}
