using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class StatelessServiceFeature
    {
        public IStatelessServiceInstance Instance { get; set; }
    }
}
