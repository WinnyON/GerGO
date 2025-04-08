using System.Net.Sockets;

namespace GerGO.Communication
{
    interface IResponder
    {
        static abstract void SendErrorMessage(NetworkStream stream, string message);
        static abstract void SendMessage(NetworkStream stream, string message);
        static abstract void SendDataMessage(NetworkStream stream, string dataMessage);
    }
}
