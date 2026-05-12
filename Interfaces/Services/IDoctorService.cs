namespace ClinicManagementSystem.Interfaces.Services;

public interface IDoctorService
{
    Task<IReadOnlyList<Doctor>> GetAllAsync();
    Task<Doctor?> GetByIdAsync(int id);
    Task<IReadOnlyList<Doctor>> GetDeletedAsync();
    Task CreateAsync(Doctor doctor);
    Task UpdateAsync(Doctor doctor);
    Task DeleteAsync(int id);
    Task RestoreAsync(int id);
}
