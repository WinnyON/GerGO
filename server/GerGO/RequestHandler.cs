using GerGO.Functionalities;
using GerGO.Utils;
using GerGO.Communication;
using System.Net.Sockets;
using System.Text;

namespace GerGO
{
    enum RequestType { EXIT, CREATE_DB, DROP_DB, CREATE_TABLE, DROP_TABLE, ALTER_TABLE, 
        GET_DB_DETAILS, GET_TABLE_DETAILS }
    class RequestHandler
    {
        private TcpClient _tcpClient;

        private Logger _logger = LoggerFactory.GetLogger();

        private static List<Command> s_commands;

        static RequestHandler()
        {
            s_commands = new List<Command>();
            s_commands.Add(new ExitCommand());
            s_commands.Add(new CreateDBCommand());
            s_commands.Add(new DropDBCommand());
            s_commands.Add(new CreateTableCommand());
            s_commands.Add(new DropTableCommand());
            s_commands.Add(new GetDBDetailsCommand());
            s_commands.Add(new GetTableDetailsCommand());
        }
        public RequestHandler(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;

            NetworkStream stream = _tcpClient.GetStream();
            _logger.Info("Thread " + Thread.CurrentThread.ManagedThreadId + " - Got request!");

            byte[] buffer = new byte[1024];
            stream.Read(buffer, 0, buffer.Length);

            string request = Encoding.UTF8.GetString(buffer);

            string[] commandArgs = request.Split("^");
            for (int i = 0; i < commandArgs.Length; i++)
            {
                commandArgs[i] = commandArgs[i].Replace("\0", "");
            }

            if (commandArgs.Length == 0)
            {
                _logger.Error("No request!");
                TcpResponder.SendErrorMessage(stream, "No request!");
                tcpClient.Close();
                return;
            }

            try
            {
                RequestType type = (RequestType)int.Parse(commandArgs[0]);

                s_commands[(int)type].Execute(stream, commandArgs);

                _logger.Info("Thread " + Thread.CurrentThread.ManagedThreadId + " - Finished command succesfully: " + type.ToString());
            }
            catch (FormatException)
            {
                _logger.Error("Error in request!");
                TcpResponder.SendErrorMessage(stream, "Error in request!");
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not valid request!");
                TcpResponder.SendErrorMessage(stream, "Not valid request!");
            }
            catch (CommandException ex)
            {
                _logger.Error("Got command exception: " + ex.Message);
                TcpResponder.SendErrorMessage(stream, "Failed to complete command: " + ex.Message);
            }
            tcpClient.Close();
        }
    }
}
