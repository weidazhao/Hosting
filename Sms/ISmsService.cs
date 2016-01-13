using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sms
{
    public interface ISmsService
    {
        Task<IEnumerable<string>> GetMessagesAsync(string user);

        Task PostMessageAsync(string user, string message);
    }
}
