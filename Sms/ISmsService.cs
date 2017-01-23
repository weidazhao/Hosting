using System.Threading.Tasks;

namespace Sms
{
    public interface ISmsService
    {
        Task<string> GetMessageAsync(string user);

        Task PostMessageAsync(string user, string message);
    }
}
