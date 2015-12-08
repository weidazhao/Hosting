using System.Fabric;
using System.Threading;

namespace Web
{
    public static class Program
    {
        public static string[] Arguments { get; set; }

        public static void Main(string[] args)
        {
            // Captures the arguments to be used in HttpCommunicationListener.
            // TODO:
            // Request to FabricRuntime to allow args to flow to Service constructor.
            Arguments = args;

            using (FabricRuntime fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterServiceType("WebType", typeof(MyStatelessService));

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
