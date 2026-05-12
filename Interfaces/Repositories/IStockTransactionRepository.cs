namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IStockTransactionRepository : IRepository<StockTransaction>
{
    Task<IReadOnlyList<StockTransaction>> GetByProductAsync(int productId);
    Task<IReadOnlyList<StockTransaction>> GetByVisitAsync(int visitId);
    Task<IReadOnlyList<StockTransaction>> GetRecentAsync(int count = 10);
}
