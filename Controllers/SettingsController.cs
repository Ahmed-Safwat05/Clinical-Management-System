namespace ClinicManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly IOperationalDataService _operationalDataService;

    public SettingsController(ISettingsService settingsService, IOperationalDataService operationalDataService)
    {
        _settingsService = settingsService;
        _operationalDataService = operationalDataService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new SettingsViewModel
        {
            ClinicName = await _settingsService.GetValue(SettingKeys.ClinicName) ?? string.Empty,
            DefaultExamPrice = await _settingsService.GetDecimal(SettingKeys.DefaultExamPrice),
            MaxDiscount = await _settingsService.GetDecimal(SettingKeys.MaxDiscount),
            AllowDiscount = await _settingsService.GetBool(SettingKeys.AllowDiscount)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _settingsService.SetValue(SettingKeys.ClinicName, model.ClinicName);
        await _settingsService.SetValue(SettingKeys.DefaultExamPrice, model.DefaultExamPrice.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _settingsService.SetValue(SettingKeys.MaxDiscount, model.MaxDiscount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _settingsService.SetValue(SettingKeys.AllowDiscount, model.AllowDiscount.ToString().ToLowerInvariant());

        TempData["SuccessMessage"] = "تم حفظ الإعدادات بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAllVisits()
    {
        await _operationalDataService.DeleteAllVisitsAsync();
        TempData["SuccessMessage"] = "تم حذف كل الزيارات";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAllAppointments()
    {
        await _operationalDataService.DeleteAllAppointmentsAsync();
        TempData["SuccessMessage"] = "تم حذف كل المواعيد";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetOperationalData()
    {
        await _operationalDataService.ResetOperationalDataAsync();
        TempData["SuccessMessage"] = "تم تصفير بيانات التشغيل";
        return RedirectToAction(nameof(Index));
    }
}
