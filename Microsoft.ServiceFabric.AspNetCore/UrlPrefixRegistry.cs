using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;

namespace Microsoft.ServiceFabric.AspNetCore
{
    public class UrlPrefixRegistry
    {
        private readonly object _lockObject = new object();
        private readonly List<Entry> _entries = new List<Entry>();

        public static readonly UrlPrefixRegistry Default = new UrlPrefixRegistry();

        public string Register(IStatefulServiceReplica replica)
        {
            if (replica == null)
            {
                throw new ArgumentNullException(nameof(replica));
            }

            lock (_lockObject)
            {
                if (_entries.Any(p => p.Replica == replica))
                {
                    throw new ArgumentException(null, nameof(replica));
                }

                string urlPrefix = $"/replica-{Guid.NewGuid()}";

                _entries.Add(new Entry() { UrlPrefix = urlPrefix, Replica = replica });

                return urlPrefix;
            }
        }

        public bool Unregister(IStatefulServiceReplica replica)
        {
            if (replica == null)
            {
                throw new ArgumentNullException(nameof(replica));
            }

            lock (_lockObject)
            {
                Entry entry = _entries.FirstOrDefault(p => p.Replica == replica);

                return entry != null ? _entries.Remove(entry) : false;
            }
        }

        public bool StartWithUrlPrefix(PathString path, out PathString remainingPath, out IStatefulServiceReplica replica)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            remainingPath = null;
            replica = null;

            foreach (Entry entry in _entries)
            {
                if (path.StartsWithSegments(entry.UrlPrefix, out remainingPath))
                {
                    replica = entry.Replica;
                    return true;
                }
            }

            return false;
        }

        private class Entry
        {
            public string UrlPrefix { get; set; }

            public IStatefulServiceReplica Replica { get; set; }
        }
    }
}