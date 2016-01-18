using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.AspNet;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Counter
{
    public class CounterService : StatefulService, ICounterService
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<long> GetCurrentAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                var counter = await StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("counter");

                using (var tx = StateManager.CreateTransaction())
                {
                    var result = await counter.GetOrAddAsync(tx, "counter", 0);

                    await tx.CommitAsync();

                    return result;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<long> IncrementAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                var counter = await StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("counter");

                using (var tx = StateManager.CreateTransaction())
                {
                    var result = await counter.AddOrUpdateAsync(tx, "counter", 1, (k, v) => v + 1);

                    await tx.CommitAsync();

                    return result;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            // Get the address dynamically allocated by Service Fabric.
            string listeningAddress = AddressUtilities.GetListeningAddress(ServiceInitializationParameters, "CounterTypeEndpoint");

            // Build an ASP.NET 5 web application that serves as the communication listener.
            var webHostBuilder = new WebHostBuilder().UseDefaultConfiguration()
                                                     .UseStartup<Startup>()
                                                     .UseUrls(listeningAddress)
                                                     .ConfigureServices(services => services.AddSingleton<ICounterService>(this));

            return new[] { new ServiceReplicaListener(_ => new AspNetCommunicationListener(webHostBuilder, AddressUtilities.GetPublishingAddress(listeningAddress))) };
        }
    }
}
