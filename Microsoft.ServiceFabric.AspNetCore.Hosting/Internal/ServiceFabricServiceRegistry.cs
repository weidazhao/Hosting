using Microsoft.AspNetCore.Http;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Immutable;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class ServiceFabricServiceRegistry
    {
        private ImmutableDictionary<PathString, object> _entries = ImmutableDictionary<PathString, object>.Empty;

        public bool TryAdd(object service, out PathString servicePathBase)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (!(service is StatelessService) && !(service is StatefulService))
            {
                throw new ArgumentException(null, nameof(service));
            }

            servicePathBase = ServicePathBase.New();

            return ImmutableInterlocked.TryAdd(ref _entries, servicePathBase, service);
        }

        public bool TryRemove(PathString servicePathBase, out object service)
        {
            if (servicePathBase == null)
            {
                throw new ArgumentNullException(nameof(servicePathBase));
            }

            return ImmutableInterlocked.TryRemove(ref _entries, servicePathBase, out service);
        }

        public bool TryGet(PathString path, out PathString servicePathBase, out PathString remainingPath, out object service)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            servicePathBase = null;
            remainingPath = null;
            service = null;

            if (!ServicePathBase.TryGet(path, out servicePathBase, out remainingPath))
            {
                return false;
            }

            if (!_entries.TryGetValue(servicePathBase, out service))
            {
                return false;
            }

            return true;
        }

        private static class ServicePathBase
        {
            private static readonly int LengthOfServicePathBase = New().Value.Length;

            public static PathString New()
            {
                return $"/sf-{Guid.NewGuid().ToString("N")}";
            }

            public static bool TryGet(PathString path, out PathString servicePathBase, out PathString remainingPath)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                servicePathBase = null;
                remainingPath = null;

                string pathValue = path.Value;

                if (string.IsNullOrEmpty(pathValue) || pathValue.Length < LengthOfServicePathBase)
                {
                    return false;
                }

                servicePathBase = pathValue.Substring(0, LengthOfServicePathBase);

                string remainingPathValue = pathValue.Substring(LengthOfServicePathBase);

                if (!string.IsNullOrEmpty(remainingPathValue) && remainingPathValue[0] != '/')
                {
                    return false;
                }

                if (remainingPathValue == "/")
                {
                    remainingPathValue = string.Empty;
                }

                remainingPath = remainingPathValue;

                return true;
            }
        }
    }
}
