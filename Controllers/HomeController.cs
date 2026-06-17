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
    public IActionResult About()
    {
        var licenseInfo = _licenseService.GetCurrentLicenseInfo();

        return View(licenseInfo);
    }

}
