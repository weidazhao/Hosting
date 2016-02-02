using System;
using System.Fabric;
using System.Threading;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class StatelessServiceInstanceCache
    {
        public static readonly StatelessServiceInstanceCache Default = new StatelessServiceInstanceCache();

        private IStatelessServiceInstance _instance;

        public bool TryAdd(IStatelessServiceInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return Interlocked.CompareExchange(ref _instance, instance, null) == null;
        }

        public bool TryRemove(IStatelessServiceInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return Interlocked.CompareExchange(ref _instance, null, instance) == instance;
        }

        public bool TryGet(out IStatelessServiceInstance instance)
        {
            return (instance = _instance) != null;
        }
    }
}
