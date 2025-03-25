using GerGO.Communication;
using GerGO.DataResource;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class GetTableDetailsCommand : Command
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            ResourceManager manager = ResourceManagerFactory.GetInstance();

            List<string[]> columnList;
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

                columnList = manager.GetTableData(dbName, tableName);

                foreach (var columnData in columnList)
                {
                    TcpResponder.SendDataMessage(stream, string.Join('^', columnData));
                    byte[] buffer = new byte[10];
                    stream.Read(buffer, 0, buffer.Length);
                }
                TcpResponder.SendMessage(stream, "OK");
            }
            catch (IOException)
            {
                _logger.Error("Error in reqest!");
                throw new CommandException("Failed to retrieve database data!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error("Failed to retrive database data! " + ex.Message);
                throw new CommandException("Failed to retrieve database data!");
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error in communication! " + ex.Message);
                throw new CommandException("Error in communication! ");
            }
        }
    }
}
