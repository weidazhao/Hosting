using System.Fabric;
using System.Threading;

namespace Sms
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterServiceType("SmsType", typeof(SmsService));

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
