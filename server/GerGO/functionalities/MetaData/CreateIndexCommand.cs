using GerGO.Communication;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class CreateIndexCommand : ICommand
    {
        private ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            try
            {
                string dbName = arguments[1].ToLower();
                string tableNname = arguments[2].ToLower();
                string indexName = arguments[3].ToLower();
                string columnName = arguments[4].ToLower();

                IResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.AddIndexFile(dbName, tableNname, indexName, columnName);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for adding index!");
                throw new CommandException("Not enough arguments provided for adding index!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to add index: {ex.Message}");
                throw new CommandException($"Failed to add index: {ex.Message}");
            }

            try
            {
                TcpResponder.SendMessage(stream, "Ok");
            }
            catch (CommunicationException ex)
            {
                _logger.Error(ex.Message);
                throw new CommandException(ex.Message);
            }
        }
    }
}
