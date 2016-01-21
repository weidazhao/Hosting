using System;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNet
{
    public static class AddressUtilities
    {
        public static string GetListeningAddress(ServiceInitializationParameters parameters, string endpointName)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            var endpoint = parameters.CodePackageActivationContext.GetEndpoint(endpointName);

            if (parameters is StatefulServiceInitializationParameters)
            {
                var statefulInitParams = (StatefulServiceInitializationParameters)parameters;

                return $"{endpoint.Protocol}://+:{endpoint.Port}/{statefulInitParams.PartitionId}/{statefulInitParams.ReplicaId}/{Guid.NewGuid()}";
            }
            else if (parameters is StatelessServiceInitializationParameters)
            {
                return $"{endpoint.Protocol}://+:{endpoint.Port}";
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static string GetPublishingAddress(string listeningAddress)
        {
            if (listeningAddress == null)
            {
                throw new ArgumentNullException(nameof(listeningAddress));
            }

            return listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
        }
    }
}
