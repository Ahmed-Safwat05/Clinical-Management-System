namespace ClinicManagementSystem.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IFinancialReportService _financialReports;

    public DashboardService(ApplicationDbContext context, IFinancialReportService financialReports)
    {
        _context = context;
        _financialReports = financialReports;
    }

    public async Task<DashboardAnalyticsViewModel> GetDashboardAsync(DateTime date)
    {
        var targetDate = date.Date;
        var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var previousMonthStart = monthStart.AddMonths(-1);
        var yearStart = new DateTime(targetDate.Year, 1, 1);
        var nextYearStart = yearStart.AddYears(1);

        var todayFinancials = await _financialReports.GetVisitPeriodSummaryAsync(targetDate, targetDate.AddDays(1));
        var monthFinancials = await _financialReports.GetVisitPeriodSummaryAsync(monthStart, nextMonthStart);
        var previousMonthFinancials = await _financialReports.GetVisitPeriodSummaryAsync(previousMonthStart, monthStart);
        var yearFinancials = await _financialReports.GetVisitPeriodSummaryAsync(yearStart, nextYearStart);

        // ممتاز جداً استبعاد الـ Voided من العد
        var monthlyVisitsCount = await _context.Visits
            .CountAsync(x => x.Date >= monthStart && x.Date < nextMonthStart && x.Status != VisitStatus.Voided);

        var yearlyVisitsCount = await _context.Visits
            .CountAsync(x => x.Date >= yearStart && x.Date < nextYearStart && x.Status != VisitStatus.Voided);

        var topDoctor = await _context.Visits
            .Include(x => x.Doctor)
            .Where(x => x.Date >= monthStart && x.Date < nextMonthStart && x.Status != VisitStatus.Voided)
            .GroupBy(x => new { x.DoctorId, x.Doctor!.Name })
            .Select(group => new
            {
                DoctorName = group.Key.Name,
                VisitsCount = group.Count()
            })
            .OrderByDescending(x => x.VisitsCount)
            .FirstOrDefaultAsync();

        // Phase 4.5: Operational Insights
        var lowStockProducts = await GetLowStockProductsAsync();
        var mostConsumedProducts = await GetMostConsumedProductsThisMonthAsync(monthStart, nextMonthStart);

        // ❌ تم إزالة حساب QueueSummary من هنا لتوفير الموارد لأنه نُقل للصفحة الرئيسية

        return new DashboardAnalyticsViewModel
        {
            Date = targetDate,
            TodayFinancials = todayFinancials,
            MonthFinancials = monthFinancials,
            YearFinancials = yearFinancials,
            MonthlyRevenue = monthFinancials.CollectedRevenue,
            PreviousMonthRevenue = previousMonthFinancials.CollectedRevenue,
            RevenueTrendPercentage = CalculateTrendPercentage(monthFinancials.CollectedRevenue, previousMonthFinancials.CollectedRevenue),
            YearlyRevenue = yearFinancials.CollectedRevenue,
            MonthlyVisitsCount = monthlyVisitsCount,
            YearlyVisitsCount = yearlyVisitsCount,
            TopDoctorName = topDoctor?.DoctorName,
            TopDoctorVisitsCount = topDoctor?.VisitsCount ?? 0,
            MonthlyRevenueChart = await _financialReports.GetDailyCollectedRevenueAsync(monthStart, nextMonthStart.AddDays(-1)),
            YearlyRevenueChart = await _financialReports.GetMonthlyCollectedRevenueAsync(targetDate.Year),
            PaymentStatus = await _financialReports.GetPaymentStatusAnalyticsAsync(),
            TopDebtors = await _financialReports.GetTopDebtorsAsync(5),
            LowStockProducts = lowStockProducts,
            MostConsumedProducts = mostConsumedProducts,
            QueueSummary = null // نمرر Null عشان ميديناش إيرور في الـ ViewModel
        };
    }   

    private static decimal CalculateTrendPercentage(decimal current, decimal previous)
    {
        if (previous <= 0)
        {
            return current > 0 ? 100m : 0m;
        }

        return Math.Round((current - previous) / previous * 100m, 2);
    }

    // Phase 4.5: Operational Insights Helper Methods

    private async Task<List<LowStockProductDto>> GetLowStockProductsAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.QuantityInStock <= p.MinimumQuantity)
            .Select(p => new LowStockProductDto
            {
                Id = p.Id,
                Name = p.Name,
                QuantityInStock = p.QuantityInStock,
                MinimumQuantity = p.MinimumQuantity
            })
            .OrderBy(p => p.QuantityInStock)
            .ToListAsync();
    }

    private async Task<List<MostConsumedProductDto>> GetMostConsumedProductsThisMonthAsync(DateTime monthStart, DateTime nextMonthStart)
    {
        return await _context.VisitProductConsumptions
            .AsNoTracking()
            .Where(vc => vc.CreatedAt >= monthStart && vc.CreatedAt < nextMonthStart)
            .GroupBy(vc => new { vc.ProductId, vc.Product!.Name })
            .Select(group => new MostConsumedProductDto
            {
                Id = group.Key.ProductId,
                ProductName = group.Key.Name,
                TotalQuantityConsumed = group.Sum(vc => vc.QuantityConsumed),
                ConsumptionCount = group.Count()
            })
            .OrderByDescending(p => p.TotalQuantityConsumed)
            .Take(5)
            .ToListAsync();
    }

    private async Task<QueueSummaryDto> GetTodayQueueSummaryAsync(DateTime targetDate)
    {
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date.Date == targetDate)
            .ToListAsync();

        return new QueueSummaryDto
        {
            WaitingCount = appointments.Count(a => a.Status == AppointmentStatus.Waiting),
            CompletedCount = appointments.Count(a => a.Status == AppointmentStatus.Done),
            CancelledCount = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            WalkInCount = appointments.Count(a => a.IsWalkIn)
        };
    }
}
