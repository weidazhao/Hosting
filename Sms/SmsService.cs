using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.AspNet;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sms
{
    public class SmsService : StatefulService, ISmsService
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<IEnumerable<string>> GetMessagesAsync(string user)
        {
            await _semaphore.WaitAsync();

            try
            {
                var messageQueue = await StateManager.GetOrAddAsync<IReliableQueue<string>>(user);

                return messageQueue.ToArray();
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
            // Build an ASP.NET 5 web application that serves as the communication listener.
            var webHostBuilder = new WebHostBuilder().UseDefaultConfiguration()
                                                     .UseStartup<Startup>()
                                                     .UseServiceFabricEndpoint(ServiceInitializationParameters, "SmsTypeEndpoint")
                                                     .ConfigureServices(services => services.AddSingleton<ISmsService>(this));

            return new[] { new ServiceReplicaListener(_ => new AspNetCommunicationListener(webHostBuilder)) };
        }
    }
}
