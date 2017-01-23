using System.Threading.Tasks;

namespace Counter
{
    public interface ICounterService
    {
        Task<long> GetCurrentAsync();

        Task<long> IncrementAsync();
    }
}
