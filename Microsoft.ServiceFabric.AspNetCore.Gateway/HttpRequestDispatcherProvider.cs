using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Gateway
{
    public class HttpRequestDispatcherProvider : CommunicationClientFactoryBase<HttpRequestDispatcher>
    {
        private readonly Func<HttpRequestDispatcher> _innerDispatcherProvider;

        public HttpRequestDispatcherProvider(IServicePartitionResolver servicePartitionResolver = null, IEnumerable<IExceptionHandler> exceptionHandlers = null, string traceId = null)
            : this(() => new HttpRequestDispatcher(), servicePartitionResolver, exceptionHandlers, traceId)
        {
        }

        public HttpRequestDispatcherProvider(Func<HttpRequestDispatcher> innerDispatcherProvider, IServicePartitionResolver servicePartitionResolver = null, IEnumerable<IExceptionHandler> exceptionHandlers = null, string traceId = null)
            : base(servicePartitionResolver, exceptionHandlers, traceId)
        {
            if (innerDispatcherProvider == null)
            {
                throw new ArgumentNullException(nameof(innerDispatcherProvider));
            }

            _innerDispatcherProvider = innerDispatcherProvider;
        }

        protected override void AbortClient(HttpRequestDispatcher dispatcher)
        {
            if (dispatcher != null)
            {
                dispatcher.Dispose();
            }
        }

        protected override Task<HttpRequestDispatcher> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
        {
            var dispatcher = _innerDispatcherProvider.Invoke();
            dispatcher.BaseAddress = new Uri(endpoint, UriKind.Absolute);

            return Task.FromResult(dispatcher);
        }

        protected override bool ValidateClient(HttpRequestDispatcher dispatcher)
        {
            return dispatcher != null && dispatcher.BaseAddress != null;
        }

        protected override bool ValidateClient(string endpoint, HttpRequestDispatcher dispatcher)
        {
            return dispatcher != null && dispatcher.BaseAddress == new Uri(endpoint, UriKind.Absolute);
        }
    }
}
