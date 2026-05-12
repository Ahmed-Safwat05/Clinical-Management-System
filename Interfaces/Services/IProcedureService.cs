namespace ClinicManagementSystem.Interfaces.Services;

public interface IProcedureService
{
    Task<IReadOnlyList<Procedure>> GetAllAsync();
    Task<Procedure?> GetByIdAsync(int id);
    Task CreateAsync(Procedure procedure);
    Task UpdateAsync(Procedure procedure);
    Task DeleteAsync(int id);
}
