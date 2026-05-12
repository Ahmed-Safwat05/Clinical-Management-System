namespace ClinicManagementSystem.Interfaces.Services;

public interface IPatientService
{
    Task<IReadOnlyList<Patient>> SearchAsync(string? searchTerm);
    Task<Patient?> GetByIdAsync(int id);
    Task<IReadOnlyList<Patient>> GetDeletedAsync();
    Task CreateAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(int id);
    Task RestoreAsync(int id);
}
