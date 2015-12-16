using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.AspNet;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web
{
    public class CounterService : StatefulService, ICounterService
    {
        private string[] _args;

        public CounterService(string[] args)
        {
            _args = args;
        }

        public async Task<long> GetCurrentAsync()
        {
            var counter = await StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("counter");

            using (var tx = StateManager.CreateTransaction())
            {
                var result = await counter.GetOrAddAsync(tx, "counter", 0);

                await tx.CommitAsync();

                return result;
            }
        }

        public async Task<long> IncrementAsync()
        {
            var counter = await StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("counter");

            using (var tx = StateManager.CreateTransaction())
            {
                var result = await counter.AddOrUpdateAsync(tx, "counter", 0, (k, v) => v + 1);

                await tx.CommitAsync();

                return result;
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var builder = new AspNetCommunicationListenerBuilder().UseStartupType(typeof(Startup))
                                                                  .UseArguments(_args)
                                                                  .UseEndpoint("WebTypeEndpoint")
                                                                  .UseService(typeof(ICounterService), this);

            yield return new ServiceReplicaListener(p => builder.Build(p));
        }
    }
}
