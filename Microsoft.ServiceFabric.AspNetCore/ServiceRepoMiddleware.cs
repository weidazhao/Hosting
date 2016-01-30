using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Internal;
using System;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore
{
    public class ServiceRepoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _services;

        public ServiceRepoMiddleware(RequestDelegate next, IServiceProvider services)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            _next = next;
            _services = services;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var service = ServiceRepo.Instance.GetService(context.Request);
            if (service.Value == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            context.Features.Set<IServiceProvidersFeature>(new MyServiceProvidersFeature(_services, service.Value));

            //
            // We found the right service instance/replica to process the request.
            //
            PathString idPath = new PathString("/" + service.Key);
            PathString remainingPath;
            if (!context.Request.Path.StartsWithSegments(idPath, out remainingPath))
            {
                context.Response.StatusCode = 404;
                return;
            }

            context.Request.PathBase = context.Request.PathBase + idPath;
            context.Request.Path = remainingPath;

            await _next.Invoke(context);
        }

        private class MyServiceProvidersFeature : IServiceProvidersFeature
        {
            private IServiceProvider _myServiceProvider;

            public MyServiceProvidersFeature(IServiceProvider inner, object service)
            {
                _myServiceProvider = new MyServiceProvider(inner, service);
            }

            public IServiceProvider RequestServices
            {
                get
                {
                    return _myServiceProvider;
                }
                set
                {
                }
            }
        }

        private class MyServiceProvider : IServiceProvider
        {
            private IServiceProvider _inner;
            private object _service;

            public MyServiceProvider(IServiceProvider inner, object service)
            {
                _inner = inner;
                _service = service;
            }

            public object GetService(Type serviceType)
            {
                var service = _inner.GetService(serviceType);

                if (service != null)
                {
                    return service;
                }

                if (serviceType.IsAssignableFrom(_service.GetType()))
                {
                    return _service;
                }

                return null;
            }
        }
    }
}
