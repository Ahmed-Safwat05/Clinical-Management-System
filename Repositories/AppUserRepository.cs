namespace ClinicManagementSystem.Repositories;

public class AppUserRepository : IAppUserRepository
{
    private readonly ApplicationDbContext _context;

    public AppUserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(x => x.Username == username && x.IsActive);
    }

    public async Task<AppUser?> GetByUsernameIncludingInactiveAsync(string username)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<AppUser?> GetByIdAsync(int id)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<AppUser>> GetAllAsync()
    {
        return await _context.AppUsers
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(AppUser user)
    {
        await _context.AppUsers.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AppUser user)
    {
        _context.AppUsers.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.AppUsers.AnyAsync(x => x.Id == id);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
