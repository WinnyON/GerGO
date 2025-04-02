using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class ExitCommand : ICommand
    {
        public void Execute(NetworkStream stream, string[] arguments)
        {
            Server.Stop();
        }
    }
}
