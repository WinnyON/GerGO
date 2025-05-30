using GerGO.Utils;

namespace GerGO
{
    class EntryPoint
    {
        public static void Main(string[] args)
        {
            Server.InitializeServer("172.20.10.2", 12000);

            Server.Start();
            Server.Run();
            // TestInserter.InsertTestData();
        }
    }
}
