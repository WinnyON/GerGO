using GerGO.Utils;

namespace GerGO
{
    class EntryPoint
    {
        public static void Main(string[] args)
        {
            Server.InitializeServer("192.168.100.15", 12000);

            Server.Start();
            Server.Run();
            // TestInserter.InsertTestData();
        }
    }
}
