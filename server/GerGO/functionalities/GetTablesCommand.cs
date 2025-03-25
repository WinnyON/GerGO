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
                _logger.Error("Wrong request format!");
                throw new CommandException("Wrong request format!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error("Failed to retrieve table data!");
                throw new CommandException("Failed to retrieve table data!");
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Failed to send table data!");
                throw new CommandException("Failed to send table data!");
            }
        }
    }
}
