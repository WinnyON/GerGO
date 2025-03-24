using GerGO.Communication;
using GerGO.DataResource;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class GetDBDetailsCommand : Command
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            ResourceManager manager = ResourceManagerFactory.GetInstance();

            List<string[]> dbList;
            try
            {
                dbList = manager.GetDBData();
                TcpResponder.SendDbData(stream, dbList);
            }
            catch (DataResourceException ex)
            {
                _logger.Error("Failed to retrive database data! " + ex.Message);
                throw new CommandException("Failed to retrieve database data!");
            }
            catch (CommandException ex)
            {
                _logger.Error("Failed to send db data! " + ex.Message);
                throw new CommandException("Failed to retrieve database data!");
            }
        }
    }
}
