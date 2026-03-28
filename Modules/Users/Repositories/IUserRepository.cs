using Joblify.Modules.Users.Entities;

namespace Joblify.Modules.Users.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User entity);
    void Update(User entity);
    void Delete(User entity);
    Task<bool> SaveChangesAsync();
}
