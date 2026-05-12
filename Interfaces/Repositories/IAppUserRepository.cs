namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IAppUserRepository
{
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<AppUser?> GetByUsernameIncludingInactiveAsync(string username);
    Task<AppUser?> GetByIdAsync(int id);
    Task<List<AppUser>> GetAllAsync();
    Task AddAsync(AppUser user);
    Task UpdateAsync(AppUser user);
    Task<bool> ExistsAsync(int id);
    Task SaveAsync();
}
