namespace ClinicManagementSystem.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index(DateTime? selectedDate)
    {
        var targetDate = selectedDate ?? DateTime.Today;
        var model = await _dashboardService.GetDashboardAsync(targetDate);
        ViewData["SelectedMonth"] = targetDate.ToString("yyyy-MM");
        return View(model);
    }
}
