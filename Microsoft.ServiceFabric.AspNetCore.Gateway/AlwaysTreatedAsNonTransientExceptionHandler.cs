using Microsoft.ServiceFabric.Services.Communication.Client;
using System;

namespace Microsoft.ServiceFabric.AspNetCore.Gateway
{
    public class AlwaysTreatedAsNonTransientExceptionHandler : IExceptionHandler
    {
        public bool TryHandleException(ExceptionInformation exceptionInformation, OperationRetrySettings retrySettings, out ExceptionHandlingResult result)
        {
            if (exceptionInformation == null)
            {
                throw new ArgumentNullException(nameof(exceptionInformation));
            }

            if (retrySettings == null)
            {
                throw new ArgumentNullException(nameof(retrySettings));
            }

            result = new ExceptionHandlingRetryResult(exceptionInformation.Exception, false, retrySettings, retrySettings.DefaultMaxRetryCount);

            return true;
        }
    }
}
