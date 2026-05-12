namespace ClinicManagementSystem.Interfaces.Services;

public interface IHomeService
{
    Task<HomeViewModel> GetHomeAsync(DateTime date);
    Task<List<RevenuePointDto>> GetRevenueChartAsync(string period, DateTime date);
}
