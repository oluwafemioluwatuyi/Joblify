using Joblify.Modules.Redis.DTOs;
using Joblify.Modules.Redis.Entities;
using Joblify.Modules.Redis.Repositories;

namespace Joblify.Modules.Redis.Services;

public class RedisService : IRedisService
{
    private readonly IRedisRepository _repository;

    public RedisService(IRedisRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RedisDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<RedisDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<RedisDto> CreateAsync(CreateRedisDto dto)
    {
        var entity = new Redis
        {
            Title       = dto.Title,
            Description = dto.Description
        };

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<RedisDto?> UpdateAsync(Guid id, UpdateRedisDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        if (dto.Title is not null)       entity.Title       = dto.Title;
        if (dto.Description is not null) entity.Description = dto.Description;
        if (dto.Status is not null)      entity.Status      = dto.Status.Value;

        _repository.Update(entity);
        await _repository.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return false;

        _repository.Delete(entity);
        return await _repository.SaveChangesAsync();
    }

    // ── Manual Mapping ────────────────────────────────────────────────────────
    private static RedisDto MapToDto(Redis entity) => new()
    {
        Id          = entity.Id,
        Title       = entity.Title,
        Description = entity.Description,
        Status      = entity.Status.ToString(),
        CreatedAt   = entity.CreatedAt,
        UpdatedAt   = entity.UpdatedAt
    };
}
