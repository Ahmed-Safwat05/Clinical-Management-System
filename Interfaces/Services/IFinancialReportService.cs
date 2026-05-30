namespace ClinicManagementSystem.Interfaces.Services;

public interface IFinancialReportService
{
    Task<FinancialSummaryDto> GetVisitPeriodSummaryAsync(DateTime startDate, DateTime endDate);
    Task<PaymentStatusAnalyticsDto> GetPaymentStatusAnalyticsAsync();
    Task<List<TopDebtorDto>> GetTopDebtorsAsync(int count = 5);
    Task<List<RevenuePointDto>> GetDailyCollectedRevenueAsync(DateTime startDate, DateTime endDate);
    Task<List<RevenuePointDto>> GetMonthlyCollectedRevenueAsync(int year);
}
