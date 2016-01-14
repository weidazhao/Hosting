using System;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNet
{
    public static class AddressUtilities
    {
        public static string GetListeningAddress(ServiceInitializationParameters parameters, string endpointName)
        {
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
            return listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
        }
    }
}
