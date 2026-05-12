namespace ClinicManagementSystem.Interfaces.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync();
    Task<int> GetLowStockCountAsync();
}
