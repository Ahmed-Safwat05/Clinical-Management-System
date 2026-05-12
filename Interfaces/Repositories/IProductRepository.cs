namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetWithLowStockAsync();
    Task<IReadOnlyList<Product>> GetAllIncludingInactiveAsync();
}
