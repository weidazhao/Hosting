using Microsoft.ServiceFabric.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace Sms
{
    public class SmsService : StatefulService, ISmsService
    {
        private readonly AspNetCoreCommunicationContext _communicationContext;
        private readonly SemaphoreSlim _semaphore;

        public SmsService(StatefulServiceContext serviceContext, AspNetCoreCommunicationContext communicationContext)
            : base(serviceContext)
        {
            _communicationContext = communicationContext;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<string> GetMessageAsync(string user)
        {
            await _semaphore.WaitAsync();

            try
            {
                var messageQueue = await StateManager.GetOrAddAsync<IReliableQueue<string>>(user);

                using (var tx = StateManager.CreateTransaction())
                {
                    var message = await messageQueue.TryDequeueAsync(tx);

                    return message.HasValue ? message.Value : string.Empty;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task PostMessageAsync(string user, string message)
        {
            await _semaphore.WaitAsync();

            try
            {
                var messageQueue = await StateManager.GetOrAddAsync<IReliableQueue<string>>(user);

                using (var tx = StateManager.CreateTransaction())
                {
                    await messageQueue.EnqueueAsync(tx, message);

                    await tx.CommitAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(_ => _communicationContext.CreateCommunicationListener(this)) };
        }
    }
}
