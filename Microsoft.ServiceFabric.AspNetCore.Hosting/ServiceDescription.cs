using System;
using System.Collections.Immutable;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class ServiceDescription
    {
        public Type ServiceType { get; set; }

        public ImmutableArray<Type> InterfaceTypes { get; set; }
    }
}
