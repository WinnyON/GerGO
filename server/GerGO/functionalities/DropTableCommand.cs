using GerGO.Communication;
using GerGO.DataResource;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class DropTableCommand : Command
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            Table table = new Table();
            try
            {
                string dbName = arguments[1].ToLower();
                string name = arguments[2].ToLower();
                table.Name = name;
                table.FileName = dbName + "_" + name + ".data";

                ResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.DropTable(dbName, table);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided!");
                throw new CommandException("Not enough arguments provided!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error("Failed to complete command: " + ex.Message);
                throw new CommandException(ex.Message);
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
