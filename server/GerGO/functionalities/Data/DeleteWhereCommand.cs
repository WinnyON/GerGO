using GerGO.Communication;
using GerGO.Manager;
using GerGO.Utils;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Functionalities.Data
{
    class DeleteWhereCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            string dbName, tableName;
            try
            {
                dbName = arguments[1];
                tableName = arguments[2];

                List<string[]> wheres = [];
                string response;
                do
                {
                    TcpResponder.SendMessage(stream, "OK");
                    byte[] buffer = new byte[1024];
                    stream.Read(buffer, 0, buffer.Length);
                    response = Encoding.UTF8.GetString(buffer);
                    if (response.StartsWith('0'))
                        break;
                    response = response.Replace("\0", string.Empty);
                    wheres.Add(response.Split('^'));
                } while (!response.StartsWith('0'));

                IResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.DeleteWhere(dbName, tableName, wheres);
                TcpResponder.SendMessage(stream, "OK");
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
