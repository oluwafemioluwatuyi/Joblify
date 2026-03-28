using Joblify.Modules.Users.DTOs;
using Joblify.Modules.Users.Entities;
using Joblify.Modules.Users.Repositories;

namespace Joblify.Modules.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var entity = new User
        {
            Title       = dto.Title,
            Description = dto.Description
        };

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto)
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
    private static UserDto MapToDto(User entity) => new()
    {
        Id          = entity.Id,
        Title       = entity.Title,
        Description = entity.Description,
        Status      = entity.Status.ToString(),
        CreatedAt   = entity.CreatedAt,
        UpdatedAt   = entity.UpdatedAt
    };
}
