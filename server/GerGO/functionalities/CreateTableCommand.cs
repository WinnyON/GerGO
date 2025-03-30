using GerGO.Communication;
using GerGO.DataResource;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class CreateTableCommand : Command
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
                manager.AddTable(dbName, table);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for creating Table!");
                throw new CommandException("Not enough arguments provided for creating Table!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to create table {arguments[2]}: {ex.Message}");
                throw new CommandException($"Failed to create table {arguments[2]}: {ex.Message}");
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
