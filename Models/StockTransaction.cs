namespace ClinicManagementSystem.Models;

public enum StockTransactionType
{
    In = 0,
    Out = 1
}

public class StockTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public StockTransactionType Type { get; set; }

    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;

    public int? VisitId { get; set; }
    public Visit? Visit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
