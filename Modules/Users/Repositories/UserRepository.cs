using Microsoft.EntityFrameworkCore;
using Joblify.Data;
using Joblify.Modules.Users.Entities;

namespace Joblify.Modules.Users.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
        => await _context.Users
                         .AsNoTracking()
                         .OrderByDescending(x => x.CreatedAt)
                         .ToListAsync();

    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users
                         .AsNoTracking()
                         .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(User entity)
        => await _context.Users.AddAsync(entity);

    public void Update(User entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(entity);
    }

    public void Delete(User entity)
        => _context.Users.Remove(entity);

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
