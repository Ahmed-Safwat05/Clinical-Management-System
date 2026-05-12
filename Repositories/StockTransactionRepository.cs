namespace ClinicManagementSystem.Repositories;

public class StockTransactionRepository : Repository<StockTransaction>, IStockTransactionRepository
{
    public StockTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<StockTransaction>> GetByProductAsync(int productId)
    {
        return await DbSet
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<StockTransaction>> GetByVisitAsync(int visitId)
    {
        return await DbSet
            .Where(x => x.VisitId == visitId)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<StockTransaction>> GetRecentAsync(int count = 10)
    {
        return await DbSet
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }
}
