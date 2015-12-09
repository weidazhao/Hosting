using System.Fabric;
using System.Threading;

namespace Web
{
    public static class Program
    {
        //
        // TODO:
        // FabricRuntime.RegisterServiceType() needs to accept a service creator as Func<T> so that the args can flow into service type.
        //
        public static string[] Arguments { get; set; }

        public static void Main(string[] args)
        {
            Arguments = args;

            using (FabricRuntime fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterServiceType("WebType", typeof(MyStatefulService));

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
