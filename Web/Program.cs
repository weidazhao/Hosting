using System.Fabric;
using System.Threading;

namespace Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (FabricRuntime fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatefulServiceFactory("WebType", () => new MyStatefulService(args));

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
