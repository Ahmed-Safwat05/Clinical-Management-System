namespace ClinicManagementSystem.Services;

public class StockManagementService : IStockManagementService
{
    private readonly IStockTransactionRepository _transactionRepository;
    private readonly IProductRepository _productRepository;

    public StockManagementService(IStockTransactionRepository transactionRepository, IProductRepository productRepository)
    {
        _transactionRepository = transactionRepository;
        _productRepository = productRepository;
    }

    public async Task<bool> AddStockAsync(int productId, int quantity, string reason, int? visitId = null)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        // Create transaction
        var transaction = new StockTransaction
        {
            ProductId = productId,
            Quantity = quantity,
            Type = StockTransactionType.In,
            Reason = reason,
            VisitId = visitId,
            CreatedAt = DateTime.UtcNow
        };

        // Update product quantity
        product.QuantityInStock += quantity;

        try
        {
            await _transactionRepository.AddAsync(transaction);
            _productRepository.Update(product);
            await _transactionRepository.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveStockAsync(int productId, int quantity, string reason, int? visitId = null)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        // Prevent negative stock
        if (product.QuantityInStock < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock. Available: {product.QuantityInStock}, Requested: {quantity}");
        }

        // Create transaction
        var transaction = new StockTransaction
        {
            ProductId = productId,
            Quantity = quantity,
            Type = StockTransactionType.Out,
            Reason = reason,
            VisitId = visitId,
            CreatedAt = DateTime.UtcNow
        };

        // Update product quantity
        product.QuantityInStock -= quantity;

        try
        {
            await _transactionRepository.AddAsync(transaction);
            _productRepository.Update(product);
            await _transactionRepository.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<StockTransaction>> GetProductTransactionsAsync(int productId)
    {
        return await _transactionRepository.GetByProductAsync(productId);
    }

    public async Task<IReadOnlyList<StockTransaction>> GetVisitTransactionsAsync(int visitId)
    {
        return await _transactionRepository.GetByVisitAsync(visitId);
    }

    public async Task<IReadOnlyList<StockTransaction>> GetRecentTransactionsAsync(int count = 10)
    {
        return await _transactionRepository.GetRecentAsync(count);
    }

    public async Task<string?> ValidateStockAvailableAsync(int productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return "المنتج غير موجود";
        }

        if (product.QuantityInStock < quantity)
        {
            return $"الكمية المتاحة {product.QuantityInStock} أقل من المطلوبة {quantity}";
        }

        return null; // No error
    }
}
