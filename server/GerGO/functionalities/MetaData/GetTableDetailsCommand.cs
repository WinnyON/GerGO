using GerGO.Communication;
using GerGO.Manager;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class GetTableDetailsCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            IResourceManager manager = ResourceManagerFactory.GetInstance();

            List<string[]> columnList;
            try
            {
                string dbName = arguments[1].ToLower();
                string tableName = arguments[2].ToLower();

                TcpResponder.SendMessage(stream, "OK");
                
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
                _logger.Error("Failed to retrieve table data!");
                throw new CommandException("Failed to retrieve table data!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to retrive table data: {ex.Message}");
                throw new CommandException($"Failed to retrive table data: {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error($"Error in communication: {ex.Message}");
                throw new CommandException($"Error in communication: {ex.Message}");
            }
        }
    }
}
