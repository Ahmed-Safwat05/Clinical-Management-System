namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IDoctorRepository : IRepository<Doctor>
{
    Task<IReadOnlyList<Doctor>> GetWithSchedulesAsync();
    Task<IReadOnlyList<Doctor>> GetDeletedAsync();
    Task<Doctor?> GetDeletedByIdAsync(int id);
    Task<int> CountAsync();
}
