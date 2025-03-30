using GerGO.Communication;
using GerGO.DataResource;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class GetDBDetailsCommand : Command
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            ResourceManager manager = ResourceManagerFactory.GetInstance();

            List<string[]> dbList;
            try
            {
                dbList = manager.GetDBData();

                foreach (var dbData in dbList)
                {
                    TcpResponder.SendDataMessage(stream, string.Join('^', dbData));
                    byte[] buffer = new byte[10];
                    stream.Read(buffer, 0, buffer.Length);
                }
                TcpResponder.SendMessage(stream, "OK");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to retrive database data: {ex.Message}");
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
