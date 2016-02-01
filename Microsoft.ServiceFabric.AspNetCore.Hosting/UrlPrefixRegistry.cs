using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class UrlPrefixRegistry
    {
        private readonly StringComparison _comparisonType;
        private readonly object _lockObject;
        private readonly SortedList<string, object> _entries;

        public static readonly UrlPrefixRegistry Default = new UrlPrefixRegistry();

        public UrlPrefixRegistry(StringComparison comparisonType = StringComparison.Ordinal)
        {
            _comparisonType = comparisonType;

            _lockObject = new object();

            _entries = new SortedList<string, object>(Comparer<string>.Create((x, y) => -string.Compare(x, y, _comparisonType)));
        }

        public string Register(object instanceOrReplica)
        {
            if (instanceOrReplica == null)
            {
                throw new ArgumentNullException(nameof(instanceOrReplica));
            }

            if (!(instanceOrReplica is IStatelessServiceInstance) && !(instanceOrReplica is IStatefulServiceReplica))
            {
                throw new ArgumentException(null, nameof(instanceOrReplica));
            }

            lock (_lockObject)
            {
                if (_entries.ContainsValue(instanceOrReplica))
                {
                    throw new ArgumentException(null, nameof(instanceOrReplica));
                }

                string urlPrefix = null;

                if (instanceOrReplica is IStatelessServiceInstance)
                {
                    urlPrefix = "/";
                }

                if (instanceOrReplica is IStatefulServiceReplica)
                {
                    urlPrefix = $"/replica-{Guid.NewGuid()}";
                }

                _entries.Add(urlPrefix, instanceOrReplica);

                return urlPrefix;
            }
        }

        public bool Unregister(object instanceOrReplica)
        {
            if (instanceOrReplica == null)
            {
                throw new ArgumentNullException(nameof(instanceOrReplica));
            }

            lock (_lockObject)
            {
                int index = _entries.IndexOfValue(instanceOrReplica);

                if (index >= 0)
                {
                    _entries.RemoveAt(index);
                    return true;
                }

                return false;
            }
        }

        public bool StartWithUrlPrefix(PathString path, out PathString urlPrefix, out PathString remainingPath, out object instanceOrReplica)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            urlPrefix = null;
            remainingPath = null;
            instanceOrReplica = null;

            lock (_lockObject)
            {
                foreach (var entry in _entries)
                {
                    if (path.StartsWithSegments(entry.Key, _comparisonType, out remainingPath))
                    {
                        urlPrefix = entry.Key;
                        instanceOrReplica = entry.Value;

                        return true;
                    }
                }

                return false;
            }
        }
    }
}
