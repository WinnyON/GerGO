using GerGO.Communication;
using GerGO.Manager;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.Data
{
    class GetAllRowsCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            IResourceManager resourceManager = ResourceManagerFactory.GetInstance();

            string dbName, tableName;
            List<string> columnNames = [];
            try
            {
                dbName = arguments[1];
                tableName = arguments[2];
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments for retrieving all rows!");
                throw new CommandException("Not enough arguments for retrieving all rows!");
            }

            for (int i = 3; i < arguments.Length; i++)
            {
                columnNames.Add(arguments[i]);
            }

            try
            {
                List<string> rows = resourceManager.GetAllRows(dbName, tableName, columnNames);
                TcpResponder.SendMessage(stream, rows.Count.ToString());
                byte[] buffer = new byte[10];
                stream.Read(buffer, 0, buffer.Length);
                foreach (string row in rows)
                {
                    TcpResponder.SendDataMessage(stream, "1^" + row);
                    buffer = new byte[10];
                    stream.Read(buffer, 0, buffer.Length);
                }
                TcpResponder.SendMessage(stream, "OK");
            }
            catch (IOException)
            {
                _logger.Error("Failed to retrieve all rows!");
                throw new CommandException("Failed to retrieve all rows!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to retrieve all rows: {ex.Message}");
                throw new CommandException($"Failed to retrieve all rows: {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error($"Error in communication: {ex.Message}");
                throw new CommandException($"Error in communication: {ex.Message}");
            }
        }
    }
}
