using System.Fabric;
using System.Threading;

namespace Counter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterServiceType("CounterType", typeof(CounterService));

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
