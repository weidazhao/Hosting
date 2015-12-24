using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.AspNet;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Web
{
    public class CounterService : StatefulService, ICounterService
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

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
                    var result = await counter.AddOrUpdateAsync(tx, "counter", 0, (k, v) => v + 1);

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
            var config = WebApplicationConfiguration.GetDefault();

            var builder = new WebApplicationBuilder().UseConfiguration(config)
                                                     .UseStartup<Startup>()
                                                     .UseServerFactory("Microsoft.AspNet.Server.Kestrel")
                                                     .ConfigureServices(services => services.AddSingleton<ICounterService>(this));

            var endpoint = ServiceInitializationParameters.CodePackageActivationContext.GetEndpoint("WebTypeEndpoint");

            yield return new ServiceReplicaListener(_ => new AspNetCommunicationListener(builder, $"{endpoint.Protocol}://+:{endpoint.Port}"));
        }
    }
}
