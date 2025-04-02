using GerGO.Communication;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class AddColumnCommand : ICommand
    {
        private ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            Column column = new Column();
            try
            {
                string dbName = arguments[1].ToLower();
                string tableName = arguments[2].ToLower();
                column.Name = arguments[3].ToLower();
                column.Type = arguments[4].ToLower();

                if (arguments[5].Equals("--"))
                    column.PrimaryKey = false;
                else
                    column.PrimaryKey = true;

                if (arguments[6].Equals("--"))
                    column.NotNull = false;
                else
                    column.NotNull = true;

                if (arguments[7].Equals("--"))
                    column.DefaultVal = string.Empty;
                else
                    column.DefaultVal = arguments[7];

                if (!arguments[8].Equals("--"))
                    column.PKIdentity.Seed = int.Parse(arguments[8]);
                if (!arguments[9].Equals("--"))
                    column.PKIdentity.Step = int.Parse(arguments[9]);

                if (arguments[10].Equals("--"))
                    column.Unique = false;
                else
                    column.Unique = true;

                if (arguments[11].Equals("--"))
                    column.Check = string.Empty;
                else
                    column.Check = arguments[11];

                IResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.AddColumn(dbName, tableName, column);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for adding column!");
                throw new CommandException("Not enough arguments provided for adding column!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to add column: {ex.Message}");
                throw new CommandException($"Failed to add column: {ex.Message}");
            }

            try
            {
                TcpResponder.SendMessage(stream, "Ok");
            }
            catch (CommunicationException ex)
            {
                _logger.Error(ex.Message);
                throw new CommandException(ex.Message);
            }
        }
    }
}
