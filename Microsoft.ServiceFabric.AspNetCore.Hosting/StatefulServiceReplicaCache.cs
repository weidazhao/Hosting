using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Immutable;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class StatefulServiceReplicaCache
    {
        public static readonly StatefulServiceReplicaCache Default = new StatefulServiceReplicaCache();

        private ImmutableDictionary<PathString, IStatefulServiceReplica> _entries = ImmutableDictionary.Create<PathString, IStatefulServiceReplica>();

        public bool TryAdd(IStatefulServiceReplica replica, out PathString replicaUrlPrefix)
        {
            if (replica == null)
            {
                throw new ArgumentNullException(nameof(replica));
            }

            replicaUrlPrefix = ReplicaUrlPrefix.GenerateReplicaUrlPrefix();

            return ImmutableInterlocked.TryAdd(ref _entries, replicaUrlPrefix, replica);
        }

        public bool TryRemove(PathString replicaUrlPrefix, out IStatefulServiceReplica replica)
        {
            if (replicaUrlPrefix == null)
            {
                throw new ArgumentNullException(nameof(replicaUrlPrefix));
            }

            return ImmutableInterlocked.TryRemove(ref _entries, replicaUrlPrefix, out replica);
        }

        public bool TryGet(PathString path, out PathString replicaUrlPrefix, out PathString remainingPath, out IStatefulServiceReplica replica)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            replicaUrlPrefix = null;
            remainingPath = null;
            replica = null;

            if (!ReplicaUrlPrefix.TryGetReplicaUrlPrefix(path, out replicaUrlPrefix))
            {
                return false;
            }

            if (!_entries.TryGetValue(replicaUrlPrefix, out replica))
            {
                return false;
            }

            return path.StartsWithSegments(replicaUrlPrefix, out remainingPath);
        }

        private static class ReplicaUrlPrefix
        {
            private static readonly string ReplicaPrefix = "sf-replica-";

            /// <summary>
            /// '/sf-replica-00000000000000000000000000000000"
            /// </summary>
            private static readonly int LengthOfReplicaUrlPrefix = 1 + ReplicaPrefix.Length + 32;

            public static PathString GenerateReplicaUrlPrefix()
            {
                return $"/{ReplicaPrefix}{Guid.NewGuid().ToString("N")}";
            }

            public static bool TryGetReplicaUrlPrefix(PathString path, out PathString replicaUrlPrefix)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                replicaUrlPrefix = null;

                if (!path.HasValue || path.Value.Length < LengthOfReplicaUrlPrefix)
                {
                    return false;
                }

                if (path.Value.Length > LengthOfReplicaUrlPrefix && path.Value[LengthOfReplicaUrlPrefix] != '/')
                {
                    return false;
                }

                replicaUrlPrefix = path.Value.Substring(0, LengthOfReplicaUrlPrefix);

                return true;
            }
        }
    }
}
