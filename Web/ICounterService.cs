using System.Threading.Tasks;

namespace Web
{
    public interface ICounterService
    {
        Task<long> GetCurrentAsync();

        Task<long> IncrementAsync();
    }
}
