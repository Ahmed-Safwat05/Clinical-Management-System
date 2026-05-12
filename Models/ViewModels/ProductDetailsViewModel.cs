namespace ClinicManagementSystem.Models.ViewModels;

public class ProductDetailsViewModel
{
    public Product? Product { get; set; }
    public IReadOnlyList<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
    public bool LowStockWarning { get; set; }
}
