using System.Net.Sockets;

namespace GerGO.Functionalities
{
    interface ICommand
    {
        void Execute(NetworkStream stream, string[] arguments);
    }
}
