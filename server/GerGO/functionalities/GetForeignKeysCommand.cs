using GerGO.Communication;
using GerGO.DataResource;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class GetForeignKeysCommand : Command
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            ResourceManager manager = ResourceManagerFactory.GetInstance();

            List<string[]> fkList;
            try
            {
                string dbName = arguments[1].ToLower();
                string tableName = arguments[2].ToLower();

                if (manager.ExistsTable(dbName, tableName))
                {
                    TcpResponder.SendMessage(stream, "OK");
                }
                else
                {
                    TcpResponder.SendErrorMessage(stream, "Fail!");
                    return;
                }
                byte[] okMesBuffer = new byte[4];
                stream.Read(okMesBuffer, 0, okMesBuffer.Length);

                fkList = manager.GetForeignKeys(dbName, tableName);

                foreach (var foreignKey in fkList)
                {
                    TcpResponder.SendDataMessage(stream, string.Join('^', foreignKey));
                    byte[] buffer = new byte[10];
                    stream.Read(buffer, 0, buffer.Length);
                }
                TcpResponder.SendMessage(stream, "OK");
            }
            catch (IOException)
            {
                _logger.Error("Failed to retrieve foreign keys!");
                throw new CommandException("Failed to retrieve foreign keys!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to retrieve foreign keys; {ex.Message}");
                throw new CommandException($"Failed to retrieve foreign keys; {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error($"Error in communication: {ex.Message}");
                throw new CommandException($"Error in communication: {ex.Message}");
            }
        }
    }
}
