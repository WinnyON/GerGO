using GerGO.Utils;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Communication
{
    class TcpResponder : Responder
    {
        private static Logger _logger = LoggerFactory.GetLogger();
        public static void SendErrorMessage(NetworkStream stream, string message)
        {
            string text = "1_" + message;
            byte[] buffer = Encoding.UTF8.GetBytes(text);

            try
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                _logger.Error("Failed to send error message!");
                throw new CommunicationException("Failed to send error message!");
            }
        }

        public static void SendMessage(NetworkStream stream, string message)
        {
            string response = "0 " + message;
            byte[] buffer = Encoding.UTF8.GetBytes(response);

            try
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                _logger.Error("Failed to send error message!");
                throw new CommunicationException("Failed to send error message!");
            }
        }
    }
}
