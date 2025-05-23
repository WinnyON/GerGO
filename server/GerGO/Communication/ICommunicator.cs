using System.Net.Sockets;

namespace GerGO.Communication
{
    interface ICommunicator
    {
        static abstract void SendErrorMessage(NetworkStream stream, string message);
        static abstract void SendMessage(NetworkStream stream, string message);
        static abstract void SendDataMessage(NetworkStream stream, string dataMessage);

        static abstract string ReadValue(NetworkStream stream);
    }
}
