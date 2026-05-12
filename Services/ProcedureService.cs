using ClinicManagementSystem.Interfaces.Repositories;
using ClinicManagementSystem.Interfaces.Services;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Services;

public class ProcedureService : IProcedureService
{
    private readonly IProcedureRepository _procedures;

    public ProcedureService(IProcedureRepository procedures)
    {
        _procedures = procedures;
    }

    public Task<IReadOnlyList<Procedure>> GetAllAsync() => _procedures.GetOrderedAsync();

    public Task<Procedure?> GetByIdAsync(int id) => _procedures.GetByIdAsync(id);

    public async Task CreateAsync(Procedure procedure)
    {
        await _procedures.AddAsync(procedure);
        await _procedures.SaveChangesAsync();
    }

    public async Task UpdateAsync(Procedure procedure)
    {
        _procedures.Update(procedure);
        await _procedures.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var procedure = await _procedures.GetByIdAsync(id);
        if (procedure is null)
        {
            return;
        }

        _procedures.Delete(procedure);
        await _procedures.SaveChangesAsync();
    }
}
