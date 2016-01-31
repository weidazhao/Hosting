using System;

namespace Microsoft.ServiceFabric.AspNetCore
{
    public class ServiceFabricFeature
    {
        public ServiceFabricFeature(Type serviceType, object instanceOrReplica)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (instanceOrReplica == null)
            {
                throw new ArgumentNullException(nameof(instanceOrReplica));
            }

            ServiceType = serviceType;
            InstanceOrReplica = instanceOrReplica;
        }

        public Type ServiceType { get; }

        public object InstanceOrReplica { get; }
    }
}
