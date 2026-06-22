using ClinicManagementSystem.DTOs.Financial;
using ClinicManagementSystem.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IPdfService _pdfService;
    private readonly IFinancialReportService _financialReportService; // 🎯 حقن السيرفيس المالية الملوكية المتاحة عندك

    public DashboardController(
        IDashboardService dashboardService,
        IPdfService pdfService,
        IFinancialReportService financialReportService)
    {
        _dashboardService = dashboardService;
        _pdfService = pdfService;
        _financialReportService = financialReportService;
    }

    public async Task<IActionResult> Index(DateTime? selectedDate)
    {
        var targetDate = selectedDate ?? DateTime.Today;
        var model = await _dashboardService.GetDashboardAsync(targetDate);
        ViewData["SelectedMonth"] = targetDate.ToString("yyyy-MM");
        return View(model);
    }

    // 📄 أكشن تحميل تقرير الإيرادات PDF المتقفل والآمن تماماً
    [HttpGet]
    public async Task<IActionResult> DownloadRevenueReport(DateTime? fromDate, DateTime? toDate, string reportType = "Daily")
    {
        // 1. تظبيط التواريخ الافتراضية بدقة
        var start = fromDate ?? DateTime.Today;
        var end = toDate ?? DateTime.Today;

        if (reportType == "Monthly")
        {
            start = new DateTime(start.Year, start.Month, 1);
            end = start.AddMonths(1).AddDays(-1);
        }

        // 2. جلب الملخص المالي للفترة من السيرفيس المالية الحقيقية
        var financialSummary = await _financialReportService.GetVisitPeriodSummaryAsync(start, end.AddDays(1));

        // 3. جلب نقاط الإيرادات التفصيلية لبناء الجدول
        var dailyPoints = await _financialReportService.GetDailyCollectedRevenueAsync(start, end);

        // 4. الحسابات والمؤشرات المالية (KPIs)
        decimal totalRevenue = financialSummary.CollectedRevenue;
        decimal totalBilled = financialSummary.BilledRevenue;
        decimal outstandingAmount = totalBilled - totalRevenue; // المستحقات = المتبقي غير المحصل

        int totalVisits = dailyPoints.Sum(p => p.VisitsCount);
        int totalPayments = dailyPoints.Count(p => p.Revenue > 0);
        decimal averageVisitRevenue = totalVisits > 0 ? totalRevenue / totalVisits : 0m;

        // 5. بناء وتغذية الـ DTO الخاص بالـ PDF
        var reportData = new FinancialReportDataDto
        {
            Title = reportType == "Daily" ? "تقرير الإيرادات والخزنة اليومي" : "تقرير الإيرادات والخزنة الشهري",
            Subtitle = reportType == "Daily"
                ? $"التاريخ: {start:yyyy-MM-dd}"
                : $"الفترة من: {start:yyyy-MM-dd} إلى: {end:yyyy-MM-dd}",
            FromDate = start,
            ToDate = end,
            GeneratedAt = DateTime.Now,

            TotalRevenue = totalRevenue,
            OutstandingAmount = outstandingAmount > 0 ? outstandingAmount : 0m,
            TotalVisits = totalVisits,
            TotalPayments = totalPayments,
            AverageVisitRevenue = averageVisitRevenue,

            // بناء الجدول بأمان كامل مع حماية تحويل الـ الأيام (Label) لـ DateTime
            Transactions = dailyPoints.Where(p => p.Revenue > 0).Select((p, index) =>
            {
                var transactionDate = start;
                if (int.TryParse(p.Label, out int dayNumber))
                {
                    // لو الـ Label رقم يوم، بنحسب تاريخه الفعلي جوه الشهر
                    transactionDate = new DateTime(start.Year, start.Month, Math.Min(dayNumber, DateTime.DaysInMonth(start.Year, start.Month)));
                }

                return new FinancialTransactionDto
                {
                    Id = (index + 1).ToString(),
                    Date = transactionDate,
                    Type = "تحصيلات الخزنة اليومية",
                    Description = $"إجمالي تحصيلات يوم {p.Label} لعدد {p.VisitsCount} زيارة مريض",
                    Amount = p.Revenue
                };
            }).ToList()
        };

        // 6. توليد ملف الـ PDF
        var fileBytes = _pdfService.GenerateFinancialReport(reportData);
        string fileName = reportType == "Daily"
            ? $"Daily_Revenue_Report_{start:yyyyMMdd}.pdf"
            : $"Monthly_Revenue_Report_{start:yyyyMM}.pdf";

        // 7. التحميل الفوري
        return File(fileBytes, "application/pdf", fileName);
    }
}