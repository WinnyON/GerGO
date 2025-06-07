using GerGO.Utils;

namespace GerGO
{
    class EntryPoint
    {
        public static void Main(string[] args)
        {
            Server.InitializeServer("192.168.100.3", 12000);

            Server.Start();
            Server.Run();
            // TestInserter.InsertTestData();
        }
    }
}
