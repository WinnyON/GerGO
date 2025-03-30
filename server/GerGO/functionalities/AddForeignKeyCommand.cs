using GerGO.Communication;
using GerGO.DataResource;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class AddForeignKeyCommand : Command
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            ForeignKey foreignKey = new ForeignKey();
            try
            {
                string dbName = arguments[1].ToLower();
                string tableNname = arguments[2].ToLower();
                foreignKey.Name = arguments[3].ToLower();
                foreignKey.RefTableName = arguments[4].ToLower();
                foreignKey.RefAttributeName = arguments[5].ToLower();
                foreignKey.AttributeName = arguments[6].ToLower();

                ResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.AddForeignKey(dbName, tableNname, foreignKey);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for adding foreign key!");
                throw new CommandException("Not enough arguments provided for adding foreign key!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to add foreign key: {ex.Message}");
                throw new CommandException($"Failed to add foreign key: {ex.Message}");
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
