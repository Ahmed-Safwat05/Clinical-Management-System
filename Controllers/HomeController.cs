namespace ClinicManagementSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IHomeService _homeService;
    private readonly ILicenseService _licenseService;

    public HomeController(IHomeService homeService, ILicenseService licenseService)
    {
        _homeService = homeService;
        _licenseService = licenseService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _homeService.GetHomeAsync(DateTime.Today);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> RevenueChart(string period = "weekly")
    {
        var points = await _homeService.GetRevenueChartAsync(period, DateTime.Today);
        return Json(new
        {
            labels = points.Select(x => x.Label),
            revenue = points.Select(x => x.Revenue),
            visits = points.Select(x => x.VisitsCount)
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public IActionResult About()
    {
        var licenseInfo = _licenseService.GetCurrentLicenseInfo();
        return View(licenseInfo);
    }

    // 🎯 الأكشن الجديدة لاستقبال كود التفعيل وتحديث الرخصة فوراً
    [HttpPost]
    public IActionResult ActivateSystem(string fullPackageKey)
    {
        if (string.IsNullOrEmpty(fullPackageKey))
        {
            TempData["LicenseError"] = "من فضلك أدخل كود التفعيل أولاً.";
            return RedirectToAction(nameof(About));
        }

        // استدعاء الميثود اللي في الـ LicenseService
        bool isActivated = _licenseService.ActivateSystemWithPackage(fullPackageKey, out string errorMessage);

        if (isActivated)
            TempData["LicenseSuccess"] = "ممتاز! تم تحديث تفعيل النظام بنجاح وفتح جميع الصلاحيات المقررة.";
        else
            TempData["LicenseError"] = errorMessage;

        return RedirectToAction(nameof(About));
    }
}