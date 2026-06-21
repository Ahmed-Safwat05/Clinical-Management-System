namespace ClinicManagementSystem.Repositories;

public class ProcedureRepository : Repository<Procedure>, IProcedureRepository
{
    public ProcedureRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Procedure>> GetOrderedAsync()
    {
        // 🎯 تصفية البيانات لعرض الإجراءات النشطة فقط وتجاهل المحذوفة "ناعماً"
        return await Context.Procedures
            .AsNoTracking()
            .Where(x => !x.IsDeleted) // 👈 السطر السحري
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}