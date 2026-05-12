namespace ClinicManagementSystem.Interfaces.Services;

public interface IStockManagementService
{
    Task<bool> AddStockAsync(int productId, int quantity, string reason, int? visitId = null);
    Task<bool> RemoveStockAsync(int productId, int quantity, string reason, int? visitId = null);
    Task<IReadOnlyList<StockTransaction>> GetProductTransactionsAsync(int productId);
    Task<IReadOnlyList<StockTransaction>> GetVisitTransactionsAsync(int visitId);
    Task<IReadOnlyList<StockTransaction>> GetRecentTransactionsAsync(int count = 10);
    Task<string?> ValidateStockAvailableAsync(int productId, int quantity);
}
