using ClinicManagementSystem.Data;
using ClinicManagementSystem.Interfaces.Services;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Services;

public class HomeService : IHomeService
{
    private const int HeavyDoctorLoadThreshold = 8;
    private readonly ApplicationDbContext _context;

    public HomeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HomeViewModel> GetHomeAsync(DateTime date)
    {
        var targetDate = date.Date;
        var todayAppointments = await GetTodayAppointmentsAsync(targetDate);
        var doctorLoads = await GetDoctorLoadsAsync(targetDate);
        var unpaidVisitsCount = await _context.Visits.CountAsync(x => !x.Paid);
        var upcomingAppointmentsCount = await CountUpcomingAppointmentsAsync(DateTime.Now);
        var heavyDoctors = doctorLoads.Where(x => x.AppointmentCount > HeavyDoctorLoadThreshold).ToList();

        return new HomeViewModel
        {
            TotalPatients = await _context.Patients.CountAsync(),
            TotalDoctors = await _context.Doctors.CountAsync(),
            TodayVisitsCount = await _context.Visits.CountAsync(x => x.Date.Date == targetDate),
            TodayRevenue = await _context.Visits
                .Where(x => x.Date.Date == targetDate)
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0m,
            TotalRevenue = await _context.Visits.SumAsync(x => (decimal?)x.TotalPrice) ?? 0m,
            TodayAppointments = todayAppointments,
            DoctorLoads = doctorLoads,
            Notifications = BuildNotifications(upcomingAppointmentsCount, unpaidVisitsCount, heavyDoctors),
            RevenueChart = await GetRevenueChartAsync("weekly", targetDate)
        };
    }

    public async Task<List<RevenuePointDto>> GetRevenueChartAsync(string period, DateTime date)
    {
        var normalizedPeriod = (period ?? "weekly").Trim().ToLowerInvariant();
        return normalizedPeriod switch
        {
            "monthly" => await BuildDailyRevenueAsync(date.Date.AddDays(-29), date.Date, "dd MMM"),
            "yearly" => await BuildMonthlyRevenueAsync(date.Year),
            _ => await BuildDailyRevenueAsync(date.Date.AddDays(-6), date.Date, "ddd dd")
        };
    }

    private async Task<List<AppointmentDto>> GetTodayAppointmentsAsync(DateTime date)
    {
        return await _context.Appointments
            .Where(x=> x.Status == AppointmentStatus.Waiting)
            .Include(x => x.Doctor)
            .Where(x => x.Date.Date == date)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.QueueNumber)
            .Take(5)
            .Select(x => new AppointmentDto
            {
                Time = x.Date.ToString("hh:mm tt"),
                DoctorName = x.Doctor!.Name,
                Specialty = x.Doctor.Specialty,
                StatusText = GetStatusText(x.Status),
                StatusBadgeClass = GetStatusBadgeClass(x.Status)
            })
            .ToListAsync();
    }

    private async Task<List<DoctorLoadDto>> GetDoctorLoadsAsync(DateTime date)
    {
        var groupedLoads = await _context.Appointments
            .Where(x => x.Status != AppointmentStatus.Cancelled)
            .Include(x => x.Doctor)
            .Where(x => x.Date.Date == date)
            .GroupBy(x => new { x.DoctorId, x.Doctor!.Name })
            .Select(group => new
            {
                group.Key.DoctorId,
                DoctorName = group.Key.Name,
                AppointmentCount = group.Count()
            })
            .OrderByDescending(x => x.AppointmentCount)
            .ToListAsync();

        var maxCount = groupedLoads.Count == 0 ? 1 : groupedLoads.Max(x => x.AppointmentCount);

        return groupedLoads.Select(x => new DoctorLoadDto
        {
            DoctorId = x.DoctorId,
            DoctorName = x.DoctorName,
            AppointmentCount = x.AppointmentCount,
            LoadPercentage = Math.Clamp((int)Math.Round((double)x.AppointmentCount / maxCount * 100), 0, 100)
        }).ToList();
    }

    private async Task<int> CountUpcomingAppointmentsAsync(DateTime now)
    {
        var oneHourLater = now.AddHours(1);
        return await _context.Appointments.CountAsync(x =>
            x.Status == AppointmentStatus.Waiting &&
            x.Date >= now &&
            x.Date <= oneHourLater);
    }

    private static List<NotificationDto> BuildNotifications(
        int upcomingAppointmentsCount,
        int unpaidVisitsCount,
        IReadOnlyCollection<DoctorLoadDto> heavyDoctors)
    {
        var notifications = new List<NotificationDto>();

        if (upcomingAppointmentsCount > 0)
        {
            notifications.Add(new NotificationDto
            {
                Title = "مواعيد قادمة خلال ساعة",
                Message = $"{upcomingAppointmentsCount} موعد يحتاج متابعة قريبة.",
                CssClass = "urgent",
                Icon = "bi-clock-fill"
            });
        }

        if (unpaidVisitsCount > 0)
        {
            notifications.Add(new NotificationDto
            {
                Title = "زيارات غير مدفوعة",
                Message = $"{unpaidVisitsCount} زيارة لم يتم سدادها بالكامل.",
                CssClass = "",
                Icon = "bi-cash-coin"
            });
        }

        foreach (var doctor in heavyDoctors)
        {
            notifications.Add(new NotificationDto
            {
                Title = "ضغط مرتفع على طبيب",
                Message = $"{doctor.DoctorName} لديه {doctor.AppointmentCount} مواعيد اليوم.",
                CssClass = "urgent",
                Icon = "bi-activity"
            });
        }

        if (notifications.Count == 0)
        {
            notifications.Add(new NotificationDto
            {
                Title = "الوضع مستقر",
                Message = "لا توجد تنبيهات تشغيلية مهمة حالياً.",
                CssClass = "success",
                Icon = "bi-check-circle-fill"
            });
        }

        return notifications;
    }

    private async Task<List<RevenuePointDto>> BuildDailyRevenueAsync(DateTime startDate, DateTime endDate, string labelFormat)
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
                Label = day.ToString(labelFormat),
                Revenue = item?.Revenue ?? 0m,
                VisitsCount = item?.VisitsCount ?? 0
            });
        }

        return points;
    }

    private async Task<List<RevenuePointDto>> BuildMonthlyRevenueAsync(int year)
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

    private static string GetStatusText(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Done => "مؤكد",
            AppointmentStatus.Cancelled => "ملغي",
            _ => "في الانتظار"
        };
    }

    private static string GetStatusBadgeClass(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Done => "confirmed",
            AppointmentStatus.Cancelled => "cancelled",
            _ => "waiting"
        };
    }
}
