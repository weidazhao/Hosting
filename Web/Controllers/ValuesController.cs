using Microsoft.AspNet.Mvc;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly ICounterService _counterService;

        public ValuesController(ICounterService counterService)
        {
            _counterService = counterService;
        }

        [HttpGet]
        public async Task<long> Get()
        {
            return await _counterService.GetCurrentAsync();
        }

        [HttpPost]
        public async Task<long> Post()
        {
            return await _counterService.IncrementAsync();
        }
    }
}
