namespace ClinicManagementSystem.Models;

public class VisitProductConsumption
{
    public int Id { get; set; }

    public int VisitId { get; set; }
    public Visit? Visit { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, int.MaxValue)]
    public int QuantityConsumed { get; set; }

    public int StockTransactionId { get; set; }
    public StockTransaction? StockTransaction { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
