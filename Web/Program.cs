using System.Fabric;
using System.Threading;

namespace Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var waitHandle = new ManualResetEvent(false);

            using (FabricRuntime fabricRuntime = FabricRuntime.Create(() => waitHandle.Set()))
            {
                fabricRuntime.RegisterStatefulServiceFactory("WebType", () => new CounterService(args));

                waitHandle.WaitOne();
            }
        }
    }
}
