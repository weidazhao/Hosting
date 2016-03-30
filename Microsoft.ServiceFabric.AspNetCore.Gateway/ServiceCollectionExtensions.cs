using Microsoft.ServiceFabric.AspNetCore.Gateway;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultHttpRequestDispatcherProvider(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton(_ => new HttpRequestDispatcherProvider(null, new[] { new ExceptionHandler() }, null));

            return services;
        }

        private sealed class ExceptionHandler : IExceptionHandler
        {
            public bool TryHandleException(ExceptionInformation exceptionInformation, OperationRetrySettings retrySettings, out ExceptionHandlingResult result)
            {
                if (exceptionInformation == null)
                {
                    result = null;

                    return false;
                }

                result = new ExceptionHandlingRetryResult(exceptionInformation.Exception, false, retrySettings, retrySettings.DefaultMaxRetryCount);

                return true;
            }
        }
    }
}
