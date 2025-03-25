using GerGO.Communication;
using GerGO.DataResource;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class DropDBCommand : Command
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            DataBase db;
            try
            {
                db = new DataBase(arguments[1]);
                ResourceManager manager = ResourceManagerFactory.GetInstance();

                manager.DropDataBase(db);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("No arguments provided!");
                throw new CommandException("No arguments provided!");
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
