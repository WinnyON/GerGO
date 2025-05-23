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

            TcpResponder.SendMessage(stream, "OK");

            string key;
            int count = 0;
            do
            {
                key = TcpResponder.ReadValue(stream);
                if (key.StartsWith('0'))
                    break;

                IResourceManager _manager = ResourceManagerFactory.GetInstance();
                try
                {
                    _manager.Delete(dbName, tableName, key);
                    TcpResponder.SendMessage(stream, "OK");
                    count++;
                }
                catch (DataResourceException)
                {
                    TcpResponder.SendDataMessage(stream, "1^You can't delete! NGGYU!");
                    continue;
                }
                catch (CommunicationException)
                {
                    continue;
                }
            } while (!key.StartsWith('0'));

            try
            {
                TcpResponder.SendMessage(stream, $"{count}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error($"Failed to send message: {ex.Message}");
            }
        }
    }
}
