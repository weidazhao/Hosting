using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Sms.Controllers
{
    [Route("api/[controller]")]
    public class SmsController
    {
        private readonly ISmsService _smsService;

        public SmsController(ISmsService smsService)
        {
            _smsService = smsService;
        }

        [HttpGet("{user}")]
        public Task<string> GetMessageAsync(string user)
        {
            return _smsService.GetMessageAsync(user);
        }

        [HttpPost("{user}")]
        public Task PostMessageAsync(string user, [FromBody]string message)
        {
            return _smsService.PostMessageAsync(user, message);
        }
    }
}
