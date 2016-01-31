namespace System.Fabric
{
    public static class FabricRuntimeExtensions
    {
        public static void RegisterStatefulServiceFactory<T>(this FabricRuntime fabricRuntime, string serviceTypeName, Func<T> factory)
            where T : IStatefulServiceReplica
        {
            fabricRuntime.RegisterStatefulServiceFactory(serviceTypeName, new RelayStatefulServiceFactory<T>(factory));
        }

        public static void RegisterStatelessServiceFactory<T>(this FabricRuntime fabricRuntime, string serviceTypeName, Func<T> factory)
            where T : IStatelessServiceInstance
        {
            fabricRuntime.RegisterStatelessServiceFactory(serviceTypeName, new RelayStatelessServiceFactory<T>(factory));
        }

        private sealed class RelayStatefulServiceFactory<T> : IStatefulServiceFactory
            where T : IStatefulServiceReplica
        {
            private readonly Func<T> _factory;

            public RelayStatefulServiceFactory(Func<T> factory)
            {
                _factory = factory;
            }

            IStatefulServiceReplica IStatefulServiceFactory.CreateReplica(string serviceTypeName, Uri serviceName, byte[] initializationData, Guid partitionId, long replicaId)
            {
                return _factory();
            }
        }

        private sealed class RelayStatelessServiceFactory<T> : IStatelessServiceFactory
            where T : IStatelessServiceInstance
        {
            private readonly Func<T> _factory;

            public RelayStatelessServiceFactory(Func<T> factory)
            {
                _factory = factory;
            }

            IStatelessServiceInstance IStatelessServiceFactory.CreateInstance(string serviceTypeName, Uri serviceName, byte[] initializationData, Guid partitionId, long instanceId)
            {
                return _factory();
            }
        }
    }
}
