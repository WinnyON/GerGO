using GerGO.Communication;
using GerGO.Manager;
using GerGO.Utils;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Functionalities.MetaData
{
    class GetIndexCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            string dbName, tableName;
            IResourceManager resourceManager = ResourceManagerFactory.GetInstance();
            try
            {
                dbName = arguments[1].ToLower();
                tableName = arguments[2].ToLower();

                TcpResponder.SendMessage(stream, "OK");
                List<string> indexData = resourceManager.GetIndexes(dbName, tableName);
                _ = TcpResponder.ReadValue(stream);

                foreach (var index in indexData)
                {
                    TcpResponder.SendDataMessage(stream, index);
                    _ = TcpResponder.ReadValue(stream);
                }
                TcpResponder.SendMessage(stream, "OK");
            }
            catch (ArgumentOutOfRangeException)
            {
                _logger.Error("Not enogh arguments provided for getting index!");
                throw new CommandException("Not enogh arguments provided for getting index!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error(ex.Message);
                throw new CommandException(ex.Message);
            }
        }
    }
}
