using GerGO.Functionalities;
using GerGO.Functionalities.MetaData;
using GerGO.Utils;
using GerGO.Communication;
using System.Net.Sockets;
using System.Text;
using GerGO.Functionalities.Data;

namespace GerGO
{
    enum RequestType {
        EXIT, 
        CREATE_DB, DROP_DB, 
        CREATE_TABLE, DROP_TABLE,
        GET_DB_DETAILS, GET_TABLE_DETAILS,
        ADD_COLUMN, 
        ADD_FOREIGN_KEY, 
        GET_TABLES, 
        GET_FOREIGN_KEYS,
        INSERT, DELETE,
        CREATE_INDEX,
        GET_ALL_ROWS
    }
    class RequestHandler
    {
        private TcpClient _tcpClient;

        private ILogger _logger = LoggerFactory.GetLogger();

        private static List<ICommand> s_commands;

        static RequestHandler()
        {
            s_commands =
            [
                new ExitCommand(),
                new CreateDBCommand(),
                new DropDBCommand(),
                new CreateTableCommand(),
                new DropTableCommand(),
                new GetDBDetailsCommand(),
                new GetTableDetailsCommand(),
                new AddColumnCommand(),
                new AddForeignKeyCommand(),
                new GetTablesCommand(),
                new GetForeignKeysCommand(),
                new InsertCommand(),
                new DeleteCommand(),
                new CreateIndexCommand(),
                new GetAllRowsCommand()
            ];
        }
        public RequestHandler(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _tcpClient.ReceiveTimeout = 15000;
            NetworkStream stream = _tcpClient.GetStream();

            _logger.Info($"Thread {Thread.CurrentThread.ManagedThreadId} - Got request!");
            string[] commandArgs;

            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    stream.Read(buffer, 0, buffer.Length);

                    string request = Encoding.UTF8.GetString(buffer);
                    
                    if (string.IsNullOrEmpty(request))
                    {
                        // this is for the client, checks if is still connected to the server
                        return;
                    }

                    commandArgs = request.Split("^");
                    for (int i = 0; i < commandArgs.Length; i++)
                    {
                        commandArgs[i] = commandArgs[i].Replace("\0", "");
                    }

                    if (commandArgs.Length == 0 || string.IsNullOrEmpty(commandArgs[0]))
                    {
                        _logger.Error("No arguments provided in the request!");
                        TcpResponder.SendErrorMessage(stream, "No arguments provided in the request!");
                        tcpClient.Close();
                        return;
                    }

                    try
                    {
                        RequestType type = (RequestType)int.Parse(commandArgs[0]);

                        s_commands[(int)type].Execute(stream, commandArgs);

                        _logger.Info($"Thread {Thread.CurrentThread.ManagedThreadId} - Finished command succesfully: {type.ToString()} - {_tcpClient.Client.RemoteEndPoint}");
                    }
                    catch (FormatException)
                    {
                        _logger.Error("Badly formatted request: not numeric request code!");
                        TcpResponder.SendErrorMessage(stream, "Badly formatted request: not numeric request code!");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        _logger.Error("Badly formatted request: not valid request code!");
                        TcpResponder.SendErrorMessage(stream, "Badly formatted request: not valid request code!");
                    }
                    catch (CommandException ex)
                    {
                        _logger.Error($"Got command exception: {ex.Message}");
                        TcpResponder.SendErrorMessage(stream, ex.Message);
                    }
                }
                catch (IOException)
                {
                    _logger.Warning($"Thread {Thread.CurrentThread.ManagedThreadId} - Connection {_tcpClient.Client.RemoteEndPoint} timed out!");
                    break;
                }
                catch (CommunicationException)
                {
                    _logger.Error($"Thread {Thread.CurrentThread.ManagedThreadId} - got communication exception,  Connection {_tcpClient.Client.RemoteEndPoint} timed out!");
                    break;
                }
            }
            _tcpClient.Close();
        }
    }
}
