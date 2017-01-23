using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Counter.Controllers
{
    [Route("api/[controller]")]
    public class CounterController
    {
        private readonly ICounterService _counterService;

        public CounterController(ICounterService counterService)
        {
            _counterService = counterService;
        }

        [HttpGet]
        public Task<long> GetAsync()
        {
            return _counterService.GetCurrentAsync();
        }

        [HttpPost]
        public Task<long> PostAsync()
        {
            return _counterService.IncrementAsync();
        }
    }
}
