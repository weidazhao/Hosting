using Microsoft.AspNetCore.Http;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Immutable;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class ServiceFabricServiceRegistry
    {
        private ImmutableDictionary<PathString, object> _entries = ImmutableDictionary.Create<PathString, object>();

        public bool TryAdd(object service, out PathString urlPrefix)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (!(service is StatelessService) && !(service is StatefulService))
            {
                throw new ArgumentException(null, nameof(service));
            }

            urlPrefix = UrlPrefix.NewUrlPrefix();

            return ImmutableInterlocked.TryAdd(ref _entries, urlPrefix, service);
        }

        public bool TryRemove(PathString urlPrefix, out object service)
        {
            if (urlPrefix == null)
            {
                throw new ArgumentNullException(nameof(urlPrefix));
            }

            return ImmutableInterlocked.TryRemove(ref _entries, urlPrefix, out service);
        }

        public bool TryGet(PathString path, out PathString urlPrefix, out PathString remainingPath, out object service)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            urlPrefix = null;
            remainingPath = null;
            service = null;

            if (!UrlPrefix.TryGetUrlPrefix(path, out urlPrefix, out remainingPath))
            {
                return false;
            }

            if (!_entries.TryGetValue(urlPrefix, out service))
            {
                return false;
            }

            return true;
        }

        //
        // Leave UrlPrefix as a nested private class inside ServiceFabricServiceRegistry,
        // as UrlPrefix.TryGetUrlPrefix() doesn't validate the value in order to achieve better performance.
        //
        private static class UrlPrefix
        {
            private static readonly int LengthOfUrlPrefix = NewUrlPrefix().Value.Length;

            public static PathString NewUrlPrefix()
            {
                return $"/sf-{Guid.NewGuid().ToString("N")}";
            }

            public static bool TryGetUrlPrefix(PathString path, out PathString urlPrefix, out PathString remainingPath)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                urlPrefix = null;
                remainingPath = null;

                if (!path.HasValue || path.Value.Length < LengthOfUrlPrefix)
                {
                    return false;
                }

                if (path.Value.Length > LengthOfUrlPrefix && path.Value[LengthOfUrlPrefix] != '/')
                {
                    return false;
                }

                urlPrefix = path.Value.Substring(0, LengthOfUrlPrefix);
                remainingPath = path.Value.Length > LengthOfUrlPrefix ? path.Value.Substring(LengthOfUrlPrefix) : string.Empty;

                return true;
            }
        }
    }
}
