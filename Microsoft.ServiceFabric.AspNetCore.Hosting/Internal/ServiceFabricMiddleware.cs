﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class ServiceFabricMiddleware
    {
        private readonly RequestDelegate _next;

        public ServiceFabricMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context, ServiceFabricServiceRegistry registry, ServiceFabricServiceScope scope)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }


            if (!registry.TryGet(context.Request.Path, out PathString servicePathBase, out PathString remainingPath, out object service))
            {
                context.Response.StatusCode = 503;
                return;
            }

            if (context.Request.Headers.TryGetValue("X-ServiceFabric-PathBase", out StringValues pathBase))
            {
                servicePathBase = pathBase.FirstOrDefault();
            }

            context.Request.PathBase = servicePathBase + context.Request.PathBase;
            context.Request.Path = remainingPath;
            scope.Service = service;

            await _next.Invoke(context);
        }
    }
}
