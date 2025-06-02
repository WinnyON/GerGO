using GerGO.Communication;
using GerGO.Manager;
using GerGO.Utils;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Functionalities.Data
{
    class InsertCommand : ICommand
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
                TcpResponder.SendErrorMessage(stream, "Error while inserting");
                _logger.Error("Not enough arguments for inserting data!");
                throw new CommandException("Not enough arguments for inserting data!");
            }

            for (int i = 3; i < arguments.Length; i++)
            {
                columnNames.Add(arguments[i]);
            }

            TcpResponder.SendMessage(stream, "OK");

            int count = 0;
            List<string> rows;
            try
            {
                do
                {
                    rows = TcpResponder.ReadValueBatched(stream);

                    if (rows.Count == 1 && rows[0].Equals("0"))
                        break; 
                    
                    count += resourceManager.Insert(dbName, tableName, columnNames, rows);
                    TcpResponder.SendMessage(stream, "OK");

                } while (!(rows.Count == 1 && rows[0].Equals("0")));

                TcpResponder.SendMessage(stream, $"{count}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error($"Error with communication: {ex.Message}");
                throw new CommandException($"Error with communication: {ex.Message}");
            }
            catch (DataResourceException)
            {
                _logger.Error("Error at inserting!");
                TcpResponder.SendDataMessage(stream, "1^Invalid data!");
            }
        }
    }
}
