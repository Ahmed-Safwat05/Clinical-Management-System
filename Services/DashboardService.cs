namespace ClinicManagementSystem.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardAnalyticsViewModel> GetDashboardAsync(DateTime date)
    {
        var targetDate = date.Date;
        var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var previousMonthStart = monthStart.AddMonths(-1);
        var yearStart = new DateTime(targetDate.Year, 1, 1);
        var nextYearStart = yearStart.AddYears(1);

        var monthlyRevenue = await _context.Visits
            .Where(x => x.Date >= monthStart && x.Date < nextMonthStart)
            .SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;

        var previousMonthRevenue = await _context.Visits
            .Where(x => x.Date >= previousMonthStart && x.Date < monthStart)
            .SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;

        var yearlyRevenue = await _context.Visits
            .Where(x => x.Date >= yearStart && x.Date < nextYearStart)
            .SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;

        var monthlyVisitsCount = await _context.Visits
            .CountAsync(x => x.Date >= monthStart && x.Date < nextMonthStart);

        var yearlyVisitsCount = await _context.Visits
            .CountAsync(x => x.Date >= yearStart && x.Date < nextYearStart);

        var topDoctor = await _context.Visits
            .Include(x => x.Doctor)
            .Where(x => x.Date >= monthStart && x.Date < nextMonthStart)
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
        var queueSummary = await GetTodayQueueSummaryAsync(targetDate);

        return new DashboardAnalyticsViewModel
        {
            Date = targetDate,
            MonthlyRevenue = monthlyRevenue,
            PreviousMonthRevenue = previousMonthRevenue,
            RevenueTrendPercentage = CalculateTrendPercentage(monthlyRevenue, previousMonthRevenue),
            YearlyRevenue = yearlyRevenue,
            MonthlyVisitsCount = monthlyVisitsCount,
            YearlyVisitsCount = yearlyVisitsCount,
            TopDoctorName = topDoctor?.DoctorName,
            TopDoctorVisitsCount = topDoctor?.VisitsCount ?? 0,
            MonthlyRevenueChart = await BuildMonthlyDailyRevenueAsync(monthStart, nextMonthStart.AddDays(-1)),
            YearlyRevenueChart = await BuildYearlyMonthlyRevenueAsync(targetDate.Year),
            LowStockProducts = lowStockProducts,
            MostConsumedProducts = mostConsumedProducts,
            QueueSummary = queueSummary
        };
    }

    private async Task<List<RevenuePointDto>> BuildMonthlyDailyRevenueAsync(DateTime startDate, DateTime endDate)
    {
        var visits = await _context.Visits
            .Where(x => x.Date.Date >= startDate && x.Date.Date <= endDate)
            .GroupBy(x => x.Date.Date)
            .Select(group => new
            {
                Date = group.Key,
                Revenue = group.Sum(x => x.TotalPrice),
                VisitsCount = group.Count()
            })
            .ToListAsync();

        var visitsByDate = visits.ToDictionary(x => x.Date);
        var points = new List<RevenuePointDto>();

        for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
        {
            visitsByDate.TryGetValue(day, out var item);
            points.Add(new RevenuePointDto
            {
                Label = day.ToString("dd"),
                Revenue = item?.Revenue ?? 0m,
                VisitsCount = item?.VisitsCount ?? 0
            });
        }

        return points;
    }

    private async Task<List<RevenuePointDto>> BuildYearlyMonthlyRevenueAsync(int year)
    {
        var visits = await _context.Visits
            .Where(x => x.Date.Year == year)
            .GroupBy(x => x.Date.Month)
            .Select(group => new
            {
                Month = group.Key,
                Revenue = group.Sum(x => x.TotalPrice),
                VisitsCount = group.Count()
            })
            .ToListAsync();

        var visitsByMonth = visits.ToDictionary(x => x.Month);
        var points = new List<RevenuePointDto>();

        for (var month = 1; month <= 12; month++)
        {
            visitsByMonth.TryGetValue(month, out var item);
            points.Add(new RevenuePointDto
            {
                Label = new DateTime(year, month, 1).ToString("MMM"),
                Revenue = item?.Revenue ?? 0m,
                VisitsCount = item?.VisitsCount ?? 0
            });
        }

        return points;
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
