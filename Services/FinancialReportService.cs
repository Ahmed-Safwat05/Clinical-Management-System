namespace ClinicManagementSystem.Services;

public class FinancialReportService : IFinancialReportService
{
    private readonly ApplicationDbContext _context;

    public FinancialReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialSummaryDto> GetVisitPeriodSummaryAsync(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;

        var summary = await _context.Visits
            .AsNoTracking()
            .Where(x => x.Date >= start && x.Date < end)
            .Select(x => new
            {
                x.TotalPrice,
                TotalPaid = x.Payments.Sum(payment => (decimal?)payment.Amount) ?? 0m
            })
            .GroupBy(_ => 1)
            .Select(group => new FinancialSummaryDto
            {
                BilledRevenue = group.Sum(x => x.TotalPrice),
                CollectedRevenue = group.Sum(x => x.TotalPaid)
            })
            .FirstOrDefaultAsync();

        return summary ?? new FinancialSummaryDto();
    }

    public async Task<PaymentStatusAnalyticsDto> GetPaymentStatusAnalyticsAsync()
    {
        var visits = _context.Visits
            .AsNoTracking()
            .Select(x => new
            {
                x.TotalPrice,
                TotalPaid = x.Payments.Sum(payment => (decimal?)payment.Amount) ?? 0m
            });

        return new PaymentStatusAnalyticsDto
        {
            FullyPaidVisitsCount = await visits.CountAsync(x => x.TotalPaid >= x.TotalPrice && x.TotalPrice > 0),
            PartiallyPaidVisitsCount = await visits.CountAsync(x => x.TotalPaid > 0 && x.TotalPaid < x.TotalPrice),
            UnpaidVisitsCount = await visits.CountAsync(x => x.TotalPaid <= 0)
        };
    }

    public async Task<List<TopDebtorDto>> GetTopDebtorsAsync(int count = 5)
    {
        var visitBalances = await _context.Visits
            .AsNoTracking()
            .Select(x => new
            {
                x.PatientId,
                PatientName = x.Patient!.Name,
                OutstandingBalance = x.TotalPrice - (x.Payments.Sum(payment => (decimal?)payment.Amount) ?? 0m)
            })
            .Where(x => x.OutstandingBalance > 0)
            .GroupBy(x => new { x.PatientId, x.PatientName })
            .Select(group => new TopDebtorDto
            {
                PatientId = group.Key.PatientId,
                PatientName = group.Key.PatientName,
                OutstandingBalance = group.Sum(x => x.OutstandingBalance)
            })
            .OrderByDescending(x => x.OutstandingBalance)
            .Take(count)
            .ToListAsync();

        return visitBalances;
    }

    public async Task<List<RevenuePointDto>> GetDailyCollectedRevenueAsync(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var endExclusive = endDate.Date.AddDays(1);

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(x => x.CreatedAt >= start && x.CreatedAt < endExclusive)
            .GroupBy(x => x.CreatedAt.Date)
            .Select(group => new
            {
                Date = group.Key,
                Revenue = group.Sum(x => x.Amount),
                VisitsCount = group.Select(x => x.VisitId).Distinct().Count()
            })
            .ToListAsync();

        var paymentsByDate = payments.ToDictionary(x => x.Date);
        var points = new List<RevenuePointDto>();

        for (var day = start; day < endExclusive; day = day.AddDays(1))
        {
            paymentsByDate.TryGetValue(day, out var item);
            points.Add(new RevenuePointDto
            {
                Label = day.ToString("dd"),
                Revenue = item?.Revenue ?? 0m,
                VisitsCount = item?.VisitsCount ?? 0
            });
        }

        return points;
    }

    public async Task<List<RevenuePointDto>> GetMonthlyCollectedRevenueAsync(int year)
    {
        var yearStart = new DateTime(year, 1, 1);
        var nextYearStart = yearStart.AddYears(1);

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(x => x.CreatedAt >= yearStart && x.CreatedAt < nextYearStart)
            .GroupBy(x => x.CreatedAt.Month)
            .Select(group => new
            {
                Month = group.Key,
                Revenue = group.Sum(x => x.Amount),
                VisitsCount = group.Select(x => x.VisitId).Distinct().Count()
            })
            .ToListAsync();

        var paymentsByMonth = payments.ToDictionary(x => x.Month);
        var points = new List<RevenuePointDto>();

        for (var month = 1; month <= 12; month++)
        {
            paymentsByMonth.TryGetValue(month, out var item);
            points.Add(new RevenuePointDto
            {
                Label = new DateTime(year, month, 1).ToString("MMM"),
                Revenue = item?.Revenue ?? 0m,
                VisitsCount = item?.VisitsCount ?? 0
            });
        }

        return points;
    }
}
