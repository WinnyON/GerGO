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

            int count = 0;
            string response;
            try
            {
                do
                {
                    TcpResponder.SendMessage(stream, "OK");
                    byte[] buffer = new byte[1024];
                    stream.Read(buffer, 0, buffer.Length);
                    response = Encoding.UTF8.GetString(buffer);
                    response = response.Replace("\0", string.Empty);

                    if (response.Equals("0"))
                        break; 
                    try
                    {
                        resourceManager.Insert(dbName, tableName, response);
                        count++;
                    }
                    catch (DataResourceException)
                    {
                        continue;
                    }

                } while (!response.Equals("0"));

                TcpResponder.SendMessage(stream, $"{count}");
            }
            catch (IOException)
            {
                _logger.Error("Failed to read data while inserting!");
                throw new CommandException("Failed to read data while inserting!");
            }
            catch (CommunicationException ex)
            {
                _logger.Error($"Failed to send data: {ex.Message}");
                throw new CommandException($"Failed to send data: {ex.Message}");
            }
        }
    }
}
