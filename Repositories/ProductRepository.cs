namespace ClinicManagementSystem.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Product>> GetWithLowStockAsync()
    {
        return await DbSet
            .Where(x => x.QuantityInStock <= x.MinimumQuantity)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> GetAllIncludingInactiveAsync()
    {
        return await DbSet
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();
    }
}
