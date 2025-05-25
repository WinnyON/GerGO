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

            int count = 0;
            List<string> keys;
            IResourceManager _manager = ResourceManagerFactory.GetInstance();
            do
            {
                keys = TcpResponder.ReadValueBatched(stream);
                if (keys.Count == 1 && keys[0].StartsWith('0'))
                    break;

                try
                {
                    count += _manager.Delete(dbName, tableName, keys);
                    TcpResponder.SendMessage(stream, "OK");
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
            } while (!(keys.Count == 1 && keys[0].StartsWith('0')));

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
