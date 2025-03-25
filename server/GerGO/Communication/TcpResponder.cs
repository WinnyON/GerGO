using GerGO.Utils;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Communication
{
    class TcpResponder : Responder
    {
        private static Logger _logger = LoggerFactory.GetLogger();

        public static void SendDbData(NetworkStream stream, List<string[]> data)
        {
            try
            {
                foreach (var item in data)
                {
                    string dbName = item[0];
                    if (item.Length == 1)
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(dbName);
                        stream.Write(buffer, 0, buffer.Length);
                        continue;
                    }
                    for (int i = 1; i < item.Length; i++)
                    {
                        string row = dbName + "^" + item[i];
                        byte[] buffer = Encoding.UTF8.GetBytes(row);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
                string endRow = "-6^END";
                byte[] endRowBytes = Encoding.UTF8.GetBytes(endRow);
                stream.Write(endRowBytes, 0, endRowBytes.Length);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Invalid response data!");
                throw new CommunicationException("Failed to send db data!");
            }
            catch (IOException)
            {
                _logger.Error("Failed to db data!");
                throw new CommunicationException("Failed to send db data!");
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
                _logger.Error("Failed to send error message!");
                throw new CommunicationException("Failed to send error message!");
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
                _logger.Error("Failed to send error message!");
                throw new CommunicationException("Failed to send error message!");
            }
        }
    }
}
