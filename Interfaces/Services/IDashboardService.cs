namespace ClinicManagementSystem.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardAnalyticsViewModel> GetDashboardAsync(DateTime date);
}
