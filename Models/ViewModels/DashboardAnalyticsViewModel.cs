namespace ClinicManagementSystem.Models.ViewModels;

public class DashboardAnalyticsViewModel
{
    public DateTime Date { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public decimal PreviousMonthRevenue { get; set; }
    public decimal RevenueTrendPercentage { get; set; }
    public int MonthlyVisitsCount { get; set; }
    public int YearlyVisitsCount { get; set; }
    public string? TopDoctorName { get; set; }
    public int TopDoctorVisitsCount { get; set; }
    public List<RevenuePointDto> MonthlyRevenueChart { get; set; } = new();
    public List<RevenuePointDto> YearlyRevenueChart { get; set; } = new();

    // Phase 4.5: Operational Insights
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    public List<MostConsumedProductDto> MostConsumedProducts { get; set; } = new();
    public QueueSummaryDto? QueueSummary { get; set; }
}

public class LowStockProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int QuantityInStock { get; set; }
    public int MinimumQuantity { get; set; }
}

public class MostConsumedProductDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = null!;
    public int TotalQuantityConsumed { get; set; }
    public int ConsumptionCount { get; set; }
}

public class QueueSummaryDto
{
    public int WaitingCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int WalkInCount { get; set; }
}
