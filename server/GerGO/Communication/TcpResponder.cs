using GerGO.Utils;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Communication
{
    class TcpResponder : ICommunicator
    {
        private static readonly ILogger _logger = LoggerFactory.GetLogger();
        private static readonly int _maxSize = 100;
        private static readonly int _maxBufferSize = 60000;

        public static int GetMaxBatchCount()
        {
            return _maxSize;
        }
        public static string ReadValue(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            try
            {
                stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                return request.Replace("\0", "");
            }
            catch (IOException)
            {
                _logger.Error("Failed to read request: IOException on the network stream!");
                throw new CommunicationException("IOException on the network stream!");
            }
        }

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

        public static void SendBatchedMessage(NetworkStream stream, List<string> messages)
        {
            if (messages.Count > _maxSize)
            {
                _logger.Error($"Batched message can't be larger than {_maxSize} messages!");
                throw new CommunicationException($"Batched message can't be larger than {_maxSize} messages!");
            }

            messages[0] = $"2#{messages[0]}";
            byte[] buffer = Encoding.UTF8.GetBytes(string.Join('#', messages));
            try
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                _logger.Error("Failed to send batched message: IOException on the network stream!");
                throw new CommunicationException("IOException on the network stream!");
            }
        }
        public static List<string> ReadValueBatched(NetworkStream stream)
        {
            byte[] buffer = new byte[_maxBufferSize];
            try
            {
                stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                return data.Replace("\0", "").Split('#').ToList();
            }
            catch (IOException)
            {
                _logger.Error("Failed to read batched data: IOException on the network stream!");
                throw new CommunicationException("IOException on the network stream!");
            }
        }
    }
}
