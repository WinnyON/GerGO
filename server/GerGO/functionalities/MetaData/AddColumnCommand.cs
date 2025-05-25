using GerGO.Communication;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;
using System.Net.Sockets;

namespace GerGO.Functionalities.MetaData
{
    class AddColumnCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            Column column = new Column();
            PrimaryKey? pKey = null;
            bool isUnique = true;
            try
            {
                string dbName = arguments[1].ToLower();
                string tableName = arguments[2].ToLower();
                column.Name = arguments[3].ToLower();
                column.Type = arguments[4].ToLower();

                if (!arguments[5].Equals("--"))
                {
                    pKey = new PrimaryKey();
                    pKey.Name = column.Name;
                }

                if (arguments[6].Equals("--"))
                    column.NotNull = false;
                else
                    column.NotNull = true;

                if (arguments[7].Equals("--"))
                    column.DefaultVal = string.Empty;
                else
                    column.DefaultVal = arguments[7];

                if (!arguments[8].Equals("--") && pKey != null)
                {
                    pKey.PKIdentity.Seed = int.Parse(arguments[8]);
                    pKey.PKIdentity.InnerSeed = int.Parse(arguments[8]);
                }
                if (!arguments[9].Equals("--") && pKey != null)
                    pKey.PKIdentity.Step = int.Parse(arguments[9]);

                if (arguments[10].Equals("--"))
                    isUnique = false;

                if (arguments[11].Equals("--"))
                    column.Check = string.Empty;
                else
                {
                    string ops = "<>=";
                    string op = "";
                    string val = "";
                    for (int i = 0; i < arguments[11].Length; i++)
                    {
                        if (ops.Contains(arguments[11][i]))
                            op = op + arguments[11][i];
                        else val = val+ arguments[11][i];
                    }
                    column.Check = op + "^" + val;
                }

                IResourceManager manager = ResourceManagerFactory.GetInstance();
                manager.AddColumn(dbName, tableName, column, pKey, isUnique);

                TcpResponder.SendMessage(stream, "Ok");
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
            catch (CommunicationException ex)
            {
                _logger.Error(ex.Message);
                throw new CommandException(ex.Message);
            }
        }
    }
}
