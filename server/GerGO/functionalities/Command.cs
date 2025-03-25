using System.Net.Sockets;

namespace GerGO.Functionalities
{
    interface Command
    {
        void Execute(NetworkStream stream, string[] arguments);
    }
}
