using ClinicManagementSystem.Data;
using ClinicManagementSystem.Interfaces.Repositories;
using ClinicManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Repositories;

public class ProcedureRepository : Repository<Procedure>, IProcedureRepository
{
    public ProcedureRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Procedure>> GetOrderedAsync()
    {
        return await Context.Procedures.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    }
}
