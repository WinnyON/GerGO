using GerGO.Communication;
using GerGO.DataResource;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class GetTablesCommand : Command
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            try
            {
                string dbName = arguments[1].ToLower();

                ResourceManager manager = ResourceManagerFactory.GetInstance();
                string[] tables = manager.GetTables(dbName);

                TcpResponder.SendDataMessage(stream, string.Join('^', tables));
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments for retrieving table data!");
                throw new CommandException("Not enough arguments for retrieving table data!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to retrieve database data: {ex.Message}");
                throw new CommandException($"Failed to retrieve database data {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error($"Failed to send database data: {ex.Message}");
                throw new CommandException($"Failed to send database data: {ex.Message}");
            }
        }
    }
}
