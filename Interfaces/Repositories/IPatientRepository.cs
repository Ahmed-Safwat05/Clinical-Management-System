namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IPatientRepository : IRepository<Patient>
{
    Task<IReadOnlyList<Patient>> SearchAsync(string? searchTerm);
    Task<IReadOnlyList<Patient>> GetDeletedAsync();
    Task<Patient?> GetDeletedByIdAsync(int id);
    Task<int> CountAsync();
}
