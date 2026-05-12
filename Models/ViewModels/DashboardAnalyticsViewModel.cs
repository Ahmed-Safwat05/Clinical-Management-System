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
}
