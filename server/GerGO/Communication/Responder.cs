using System.Net.Sockets;

namespace GerGO.Communication
{
    interface Responder
    {
        static abstract void SendErrorMessage(NetworkStream stream, string message);
        static abstract void SendMessage(NetworkStream stream, string message);
        static abstract void SendDbData(NetworkStream stream , List<string[]> data);
    }
}
