using GerGO.Communication;
using GerGO.Manager;
using GerGO.Models;
using GerGO.Utils;
using SharpCompress.Common;
using System.Net.Sockets;
using System.Text;

namespace GerGO.Functionalities.Data
{
    class SelectCommand : ICommand
    {
        private readonly ILogger _logger = LoggerFactory.GetLogger();
        public void Execute(NetworkStream stream, string[] arguments)
        {
            SelectData selectData = new SelectData();
            try
            {
                selectData.DbName = arguments[1].ToLower();
                selectData.TableName = arguments[2].ToLower();

                string inputString;
                do
                {
                    TcpResponder.SendMessage(stream, "OK");
                    inputString = TcpResponder.ReadValue(stream);
                    string[] data = inputString.Split('^');
                    int cmdType = int.Parse(data[0]);
                    switch (cmdType)
                    {
                        case 0:
                            break;
                        case 20:
                            selectData.JoinTables.Add(data);
                            break;
                        case 21:
                            selectData.WhereClauses.Add(data);
                            break;
                        case 22:
                            selectData.GroupByClauses.Add(data);
                            break;
                        case 23:
                            selectData.HavingClauses.Add(data);
                            break;
                        case 24:
                            selectData.OrderByCluases.Add(data);
                            break;
                        case 25:
                            selectData.Columns.Add(data);
                            break;
                        default:
                            throw new CommandException("Invalid arguments in select!");
                    }
                } while (!inputString.StartsWith('0'));

                IResourceManager resourceManager = ResourceManagerFactory.GetInstance();
                string colNames = string.Empty;
                List<string> result = resourceManager.Select(selectData, ref colNames);

                byte[] buffer = new byte[50];
                if (string.IsNullOrEmpty(colNames))
                {
                    TcpResponder.SendMessage(stream, $"{result.Count}");
                }
                else
                {
                    TcpResponder.SendMessage(stream, $"{result.Count}^{colNames}");
                }
                _ = TcpResponder.ReadValue(stream);

                foreach (var row in result)
                {
                    TcpResponder.SendDataMessage(stream, $"1^{row}");
                    _ = TcpResponder.ReadValue(stream);
                }
                TcpResponder.SendMessage(stream, "OK");
            }
            catch (IOException)
            {
                _logger.Error("Error with the communication, failed to complete Select query!");
            }
            catch (IndexOutOfRangeException)
            {
                _logger.Error("Not enough arguments provided for select!");
                throw new CommandException("Not enough arguments provided for select!");
            }
            catch (FormatException)
            {
                _logger.Error("Invalid argument provided for select!");
                throw new CommandException("Invalid argument provided for select!");
            }
            catch (DataResourceException ex)
            {
                _logger.Error($"Failed to execute query: {ex.Message}");
                throw new CommandException($"Failed to execute query: {ex.Message}");
            }
            catch (CommandException)
            {
                _logger.Error("Error with the communication, failed to complete Select query!");
            }
        }
    }
}
