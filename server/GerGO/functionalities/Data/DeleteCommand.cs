using GerGO.Communication;
using GerGO.Manager;
using GerGO.Utils;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Functionalities.Data
{
    class DeleteCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            string dbName, tableName;
            try
            {
                dbName = arguments[1];
                tableName = arguments[2];
            }
            catch (IndexOutOfRangeException)
            {
                TcpResponder.SendErrorMessage(stream, "Error while deleting");
                _logger.Error("Not enough arguments for deleting data!");
                throw new CommandException("Not enough arguments for deleting data!");
            }
            catch (IOException)
            {
                _logger.Error("Failed to send response!");
                throw new CommandException("Failed to send response!");
            }

            TcpResponder.SendMessage(stream, "OK");

            string response;
            int count = 0;
            do
            {
                byte[] buffer = new byte[1024];
                stream.Read(buffer, 0, buffer.Length);
                response = Encoding.UTF8.GetString(buffer);
                if (response.StartsWith('0'))
                    break;
                string key = response.Replace("\0", string.Empty);

                IResourceManager _manager = ResourceManagerFactory.GetInstance();
                try
                {
                    _manager.Delete(dbName, tableName, key);
                    TcpResponder.SendMessage(stream, "OK");
                    count++;
                }
                catch (DataResourceException)
                {
                    TcpResponder.SendMessage(stream, "OK");
                    continue;
                }
            } while (!response.StartsWith('0'));

            try
            {
                TcpResponder.SendMessage(stream, $"{count}");
            }
            catch (CommandException ex)
            {
                _logger.Error($"Failed to send message: {ex.Message}");
            }
        }
    }
}
