using GerGO.Communication;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class DropDBCommand : ICommand
    {
        private ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            DataBase db;
            try
            {
                db = new DataBase(arguments[1]);
                IResourceManager manager = ResourceManagerFactory.GetInstance();

                manager.DropDataBase(db);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for dropping DB!");
                throw new CommandException("Not enough arguments provided for dropping DB!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to drop DB {arguments[1]}: {ex.Message}");
                throw new CommandException($"Failed to drop DB {arguments[1]}: {ex.Message}");
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
