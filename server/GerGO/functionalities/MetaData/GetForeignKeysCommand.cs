using GerGO.Communication;
using GerGO.Manager;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class GetForeignKeysCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            IResourceManager manager = ResourceManagerFactory.GetInstance();

            List<string[]> fkList;
            try
            {
                string dbName = arguments[1].ToLower();
                string tableName = arguments[2].ToLower();

                TcpResponder.SendMessage(stream, "OK");

                _ = TcpResponder.ReadValue(stream);

                fkList = manager.GetForeignKeys(dbName, tableName);

                foreach (var foreignKey in fkList)
                {
                    TcpResponder.SendDataMessage(stream, string.Join('^', foreignKey));
                    _ = TcpResponder.ReadValue(stream);
                }
                TcpResponder.SendMessage(stream, "OK");
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments for getting all row!");
                throw new CommandException("Not enough arguments for getting all row!");
            }
            catch (IOException)
            {
                _logger.Error("Failed to retrieve foreign keys!");
                throw new CommandException("Failed to retrieve foreign keys!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to retrieve foreign keys: {ex.Message}");
                throw new CommandException($"Failed to retrieve foreign keys: {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error($"Error in communication: {ex.Message}");
                throw new CommandException($"Error in communication: {ex.Message}");
            }
        }
    }
}
