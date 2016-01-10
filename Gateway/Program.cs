using System.Fabric;
using System.Threading;

namespace Gateway
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterServiceType("GatewayType", typeof(GatewayService));

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
