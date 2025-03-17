using System.Threading.Tasks;

namespace Shared.Redis;

public interface IRedisService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, int expirationMinutes = 60);
    Task RemoveAsync(string key);
} 