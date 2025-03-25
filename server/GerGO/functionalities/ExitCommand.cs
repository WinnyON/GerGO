using System.Net.Sockets;

namespace GerGO.Functionalities
{
    class ExitCommand : Command
    {
        public void Execute(NetworkStream stream, string[] arguments)
        {
            Server.Stop();
        }
    }
}
