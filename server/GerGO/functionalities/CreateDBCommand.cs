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
                string name = arguments[1].ToLower();
                db = new DataBase(name);
                ResourceManager manager = ResourceManagerFactory.GetInstance();

                manager.AddDataBase(db);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for creating DB!");
                throw new CommandException("Not enough arguments provided for creating DB!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to create DB {arguments[1]}: {ex.Message}");
                throw new CommandException($"Failed to create DB {arguments[1]}: {ex.Message}");
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
