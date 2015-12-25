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
        public Task<long> Get()
        {
            return _counterService.GetCurrentAsync();
        }

        [HttpPost]
        public Task<long> Post()
        {
            return _counterService.IncrementAsync();
        }
    }
}
