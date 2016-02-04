using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Immutable;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class ServiceFabricRegistry
    {
        public static readonly ServiceFabricRegistry Default = new ServiceFabricRegistry();

        private ImmutableDictionary<PathString, object> _entries = ImmutableDictionary.Create<PathString, object>();

        public bool TryAdd(object instanceOrReplica, out PathString urlPrefix)
        {
            if (instanceOrReplica == null)
            {
                throw new ArgumentNullException(nameof(instanceOrReplica));
            }

            if (!(instanceOrReplica is IStatelessServiceInstance) && !(instanceOrReplica is IStatefulServiceReplica))
            {
                throw new ArgumentException(null, nameof(instanceOrReplica));
            }

            urlPrefix = UrlPrefix.NewUrlPrefix();

            return ImmutableInterlocked.TryAdd(ref _entries, urlPrefix, instanceOrReplica);
        }

        public bool TryRemove(PathString urlPrefix, out object instanceOrReplica)
        {
            if (urlPrefix == null)
            {
                throw new ArgumentNullException(nameof(urlPrefix));
            }

            return ImmutableInterlocked.TryRemove(ref _entries, urlPrefix, out instanceOrReplica);
        }

        public bool TryGet(PathString path, out PathString urlPrefix, out PathString remainingPath, out object instanceOrReplica)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            urlPrefix = null;
            remainingPath = null;
            instanceOrReplica = null;

            if (!UrlPrefix.TryGetUrlPrefix(path, out urlPrefix, out remainingPath))
            {
                return false;
            }

            if (!_entries.TryGetValue(urlPrefix, out instanceOrReplica))
            {
                return false;
            }

            return true;
        }

        //
        // Leave UrlPrefix as a nested private class inside ServiceFabricRegistry,
        // since UrlPrefix.TryGetUrlPrefix() doesn't validate format for perf reasons.
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
