using GerGO.Communication;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class CreateIndexCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            try
            {
                string dbName = arguments[1].ToLower();
                string tableNname = arguments[2].ToLower();

                IndexFile iFile = new IndexFile();
                iFile.Name = arguments[3].ToLower();
                for (int i = 4; i < arguments.Length; i++)
                {
                    iFile.Attributes.Add(arguments[i].ToLower());
                }

                IResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.AddIndexFile(dbName, tableNname, iFile);

                TcpResponder.SendMessage(stream, "Ok");
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for adding index!");
                throw new CommandException("Not enough arguments provided for adding index!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to add index: {ex.Message}");
                throw new CommandException($"Failed to add index: {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _logger.Error(ex.Message);
                throw new CommandException(ex.Message);
            }
        }
    }
}
