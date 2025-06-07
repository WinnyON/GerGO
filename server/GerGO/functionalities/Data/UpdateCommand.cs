using GerGO.Communication;
using GerGO.Manager;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.Data
{
    class UpdateCommand : ICommand
    {
        public void Execute(NetworkStream stream, string[] arguments)
        {
            ILogger _logger = LoggerFactory.GetLogger();
            string dbName, tableName, colName, newVal;
            try
            {
                dbName = arguments[1];
                tableName = arguments[2];
                colName = arguments[3];
                newVal = arguments[4];

                List<string[]> wheres = [];
                string response;
                do
                {
                    TcpResponder.SendMessage(stream, "OK");
                    response = TcpResponder.ReadValue(stream);
                    if (response.StartsWith('0'))
                        break;
                    response = response.Replace("\0", string.Empty);
                    wheres.Add(response.Split('^'));
                } while (!response.StartsWith('0'));

                IResourceManager manager = ResourceManagerFactory.GetInstance();
                int count = manager.Update(dbName, tableName, wheres, colName, newVal);
                TcpResponder.SendMessage(stream, $"{count}");
            }
            catch (DataResourceException ex)
            {
                TcpResponder.SendErrorMessage(stream, "Error while deleting!");
                _logger.Error($"Error with deleting: {ex.Message}");
                throw new CommandException(ex.Message);
            }
            catch (CommunicationException)
            {
                _logger.Error("Error with communication!");
                throw new Exception("Error with communication!");
            }
            catch (IndexOutOfRangeException)
            {
                TcpResponder.SendErrorMessage(stream, "Error while deleting");
                _logger.Error("Not enough arguments for deleting data!");
                throw new CommandException("Not enough arguments for deleting data!");
            }
            catch (IOException)
            {
                _logger.Error("Failed to send response!");
                throw new CommandException("Failed to send response!");
            }
        }
    }
}
