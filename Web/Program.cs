using System.Fabric;
using System.Threading;

namespace Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatefulServiceFactory("WebType", () => new CounterService(args));

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
