using GerGO.Utils;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Communication
{
    class TcpResponder : IResponder
    {
        private static readonly ILogger _logger = LoggerFactory.GetLogger();

        public static void SendDataMessage(NetworkStream stream, string dataMessage)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(dataMessage);

            try
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                _logger.Error("Failed to send dataMessage: IOException on the network stream!");
                throw new CommunicationException("IOException on the network stream!");
            }
        }

        public static void SendErrorMessage(NetworkStream stream, string message)
        {
            string text = "1^" + message;
            byte[] buffer = Encoding.UTF8.GetBytes(text);

            try
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                _logger.Error("Failed to send error message: IOException on the network stream!");
                throw new CommunicationException("IOException on the network stream!");
            }
        }

        public static void SendMessage(NetworkStream stream, string message)
        {
            string response = "0^" + message;
            byte[] buffer = Encoding.UTF8.GetBytes(response);

            try
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                _logger.Error("Failed to send message: IOException on the network stream!");
                throw new CommunicationException("IOException on the network stream!");
            }
        }
    }
}
