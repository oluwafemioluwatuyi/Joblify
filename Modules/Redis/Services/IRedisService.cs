using Joblify.Modules.Redis.DTOs;

namespace Joblify.Modules.Redis.Services;

public interface IRedisService
{
    Task<IEnumerable<RedisDto>> GetAllAsync();
    Task<RedisDto?> GetByIdAsync(Guid id);
    Task<RedisDto> CreateAsync(CreateRedisDto dto);
    Task<RedisDto?> UpdateAsync(Guid id, UpdateRedisDto dto);
    Task<bool> DeleteAsync(Guid id);
}
