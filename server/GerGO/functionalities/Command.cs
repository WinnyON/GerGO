using System.Net.Sockets;

namespace GerGO.functionalities
{
    interface Command
    {
        int Execute(NetworkStream stream, string[] arguments);
    }
}
