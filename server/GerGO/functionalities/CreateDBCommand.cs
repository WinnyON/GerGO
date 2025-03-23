using System.Net.Sockets;
using GerGO.Models;
using GerGO.DataResource;
using GerGO.Utils;
using GerGO.Communication;

namespace GerGO.Functionalities
{
    class CreateDBCommand : Command
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            DataBase db;
            try
            {
                db = new DataBase(arguments[1]);
                ResourceManager manager = ResourceManagerFactory.GetInstance();

                manager.AddDataBase(db);
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
