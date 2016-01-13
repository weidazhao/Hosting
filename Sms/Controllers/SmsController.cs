using Microsoft.AspNet.Mvc;
using System.Collections.Generic;
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
        public Task<IEnumerable<string>> GetMessagesAsync(string user)
        {
            return _smsService.GetMessagesAsync(user);
        }

        [HttpPost("{user}/{message}")]
        public Task PostMessageAsync(string user, string message)
        {
            return _smsService.PostMessageAsync(user, message);
        }
    }
}
