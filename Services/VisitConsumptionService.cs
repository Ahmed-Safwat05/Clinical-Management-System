namespace ClinicManagementSystem.Services;

public class VisitConsumptionService : IVisitConsumptionService
{
    private readonly IVisitProductConsumptionRepository _consumptionRepository;
    private readonly IStockManagementService _stockManagementService;
    private readonly IProductRepository _productRepository;
    private readonly IStockTransactionRepository _transactionRepository;

    public VisitConsumptionService(
        IVisitProductConsumptionRepository consumptionRepository,
        IStockManagementService stockManagementService,
        IProductRepository productRepository,
        IStockTransactionRepository transactionRepository)
    {
        _consumptionRepository = consumptionRepository;
        _stockManagementService = stockManagementService;
        _productRepository = productRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<bool> ConsumeProductAsync(int visitId, int productId, int quantity)
    {
        // Validate stock availability
        var validationError = await _stockManagementService.ValidateStockAvailableAsync(productId, quantity);
        if (validationError != null)
        {
            throw new InvalidOperationException(validationError);
        }

        try
        {
            // Remove stock (this creates a transaction automatically)
            var success = await _stockManagementService.RemoveStockAsync(
                productId,
                quantity,
                $"استهلاك في الزيارة #{visitId}",
                visitId);

            if (!success)
            {
                throw new InvalidOperationException("Failed to remove stock");
            }

            // Get the stock transaction created for this visit/product consumption.
            var latestTransaction = await _transactionRepository.GetLatestForVisitProductAsync(visitId, productId);

            if (latestTransaction == null)
            {
                throw new InvalidOperationException("Failed to retrieve transaction");
            }

            // Create consumption record
            var consumption = new VisitProductConsumption
            {
                VisitId = visitId,
                ProductId = productId,
                QuantityConsumed = quantity,
                StockTransactionId = latestTransaction.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _consumptionRepository.AddAsync(consumption);
            await _consumptionRepository.SaveChangesAsync();

            return true;
        }
        catch
        {
            throw;
        }
    }

    public async Task<IReadOnlyList<VisitProductConsumption>> GetVisitConsumptionsAsync(int visitId)
    {
        return await _consumptionRepository.GetByVisitAsync(visitId);
    }

    public async Task<decimal> GetTotalConsumptionCostAsync(int visitId)
    {
        return await _consumptionRepository.GetTotalCostAsync(visitId);
    }
}
