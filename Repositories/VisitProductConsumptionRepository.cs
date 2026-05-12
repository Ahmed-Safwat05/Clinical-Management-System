namespace ClinicManagementSystem.Repositories;

public class VisitProductConsumptionRepository : Repository<VisitProductConsumption>, IVisitProductConsumptionRepository
{
    public VisitProductConsumptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<VisitProductConsumption>> GetByVisitAsync(int visitId)
    {
        return await DbSet
            .Where(x => x.VisitId == visitId)
            .Include(x => x.Product)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<decimal> GetTotalCostAsync(int visitId)
    {
        return await DbSet
            .Where(x => x.VisitId == visitId)
            .Include(x => x.Product)
            .SumAsync(x => (decimal)x.QuantityConsumed * x.Product!.CostPrice);
    }
}
