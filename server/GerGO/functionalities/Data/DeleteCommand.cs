using GerGO.Communication;
using GerGO.Manager;
using GerGO.Utils;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Functionalities.Data
{
    class DeleteCommand : ICommand
    {
        private ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            IResourceManager resourceManager = ResourceManagerFactory.GetInstance();

            string dbName, tableName;
            try
            {
                dbName = arguments[1];
                tableName = arguments[2];
                TcpResponder.SendMessage(stream, "OK");
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

            byte[] buffer = new byte[1024];
            stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer);
            response = response.Replace("\0", string.Empty);
            string[] keys = response.Split('^');

            IResourceManager _manager = ResourceManagerFactory.GetInstance();
            int count = 0;
            foreach (string key in keys)
            {
                try
                {
                    _manager.Delete(dbName, tableName, key);
                    count++;
                }
                catch (DataResourceException)
                {
                    continue;
                }
            }

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
