using GerGO.Communication;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class DropForeignKeyCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            try
            {
                string dbName = arguments[1].ToLower();
                string tableName = arguments[2].ToLower();
                string fkName = arguments[3].ToLower();

                IResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.DropForeignKey(dbName, tableName, fkName);

                TcpResponder.SendMessage(stream, "Ok");
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for dropping foreign key!");
                throw new CommandException("Not enough arguments provided for dropping foreign key!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to drop foreign key {arguments[3]}: {ex.Message}");
                throw new CommandException($"Failed to drop foreign key {arguments[3]}: {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error(ex.Message);
                throw new CommandException(ex.Message);
            }
        }
    }
}
