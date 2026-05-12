namespace ClinicManagementSystem.Repositories;

public class VisitRepository : Repository<Visit>, IVisitRepository
{
    public VisitRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Visit>> GetRecentAsync(int count = 50)
    {
        return await Context.Visits
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .AsNoTracking()
            .OrderByDescending(x => x.Date)
            .Take(count)
            .ToListAsync();
    }

    public Task<Visit?> GetDetailsAsync(int id)
    {
        return Context.Visits
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.VisitProcedures)
            .ThenInclude(x => x.Procedure)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<decimal> GetTotalIncomeAsync()
    {
        return await Context.Visits.SumAsync(x => (decimal?)x.PaidAmount) ?? 0m;
    }

    public async Task<decimal> GetIncomeByDateAsync(DateTime date)
    {
        var targetDate = date.Date;
        return await Context.Visits
            .Where(x => x.Date.Date == targetDate)
            .SumAsync(x => (decimal?)x.PaidAmount) ?? 0m;
    }

    public Task<int> CountPatientsByVisitDateAsync(DateTime date)
    {
        var targetDate = date.Date;
        return Context.Visits
            .Where(x => x.Date.Date == targetDate)
            .Select(x => x.PatientId)
            .Distinct()
            .CountAsync();
    }

}
