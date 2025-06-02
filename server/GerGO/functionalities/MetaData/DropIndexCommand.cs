using GerGO.Communication;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class DropIndexCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            try
            {
                string dbName = arguments[1].ToLower();
                string tableName = arguments[2].ToLower();
                string indexName = arguments[3].ToLower();

                IResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.DropIndex(dbName, tableName, indexName);

                TcpResponder.SendMessage(stream, "Ok");
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for dropping index!");
                throw new CommandException("Not enough arguments provided for dropping index!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to drop index {arguments[3]}: {ex.Message}");
                throw new CommandException($"Failed to drop index {arguments[3]}: {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error(ex.Message);
                throw new CommandException(ex.Message);
            }
        }
    }
}
