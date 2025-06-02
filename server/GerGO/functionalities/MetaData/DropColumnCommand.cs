using GerGO.Communication;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class DropColumnCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            try
            {
                string dbName = arguments[1].ToLower();
                string tableName = arguments[2].ToLower();
                string colName = arguments[3].ToLower();

                IResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.DropColumn(dbName, tableName, colName);

                TcpResponder.SendMessage(stream, "Ok");
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for dropping column!");
                throw new CommandException("Not enough arguments provided for dropping column!");
            }
            catch (DataResourceException ex)
            {
                if (ex.Message.StartsWith('2'))
                {
                    TcpResponder.SendDataMessage(stream, ex.Message);
                    return;
                }

                _logger.Error($"Failed to drop column {arguments[3]}: {ex.Message}");
                throw new CommandException($"Failed to drop column {arguments[3]}: {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error(ex.Message);
                throw new CommandException(ex.Message);
            }
        }
    }
}
