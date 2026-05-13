namespace ClinicManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly IOperationalDataService _operationalDataService;
    private readonly IAuditService _auditService;

    public SettingsController(
        ISettingsService settingsService,
        IOperationalDataService operationalDataService,
        IAuditService auditService)
    {
        _settingsService = settingsService;
        _operationalDataService = operationalDataService;
        _auditService = auditService;
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

        var currentClinicName = await _settingsService.GetValue(SettingKeys.ClinicName) ?? string.Empty;
        var currentDefaultExamPrice = await _settingsService.GetDecimal(SettingKeys.DefaultExamPrice);
        var currentMaxDiscount = await _settingsService.GetDecimal(SettingKeys.MaxDiscount);
        var currentAllowDiscount = await _settingsService.GetBool(SettingKeys.AllowDiscount);

        await _settingsService.SetValue(SettingKeys.ClinicName, model.ClinicName);
        await _settingsService.SetValue(SettingKeys.DefaultExamPrice, model.DefaultExamPrice.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _settingsService.SetValue(SettingKeys.MaxDiscount, model.MaxDiscount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _settingsService.SetValue(SettingKeys.AllowDiscount, model.AllowDiscount.ToString().ToLowerInvariant());

        if (!string.Equals(currentClinicName, model.ClinicName, StringComparison.Ordinal))
        {
            await _auditService.LogAsync(
                AuditActionType.Update,
                "Settings",
                "Changed clinic name");
        }

        if (currentDefaultExamPrice != model.DefaultExamPrice)
        {
            await _auditService.LogAsync(
                AuditActionType.Update,
                "Settings",
                $"Updated default examination price from {currentDefaultExamPrice:N2} to {model.DefaultExamPrice:N2}");
        }

        if (currentMaxDiscount != model.MaxDiscount)
        {
            await _auditService.LogAsync(
                AuditActionType.Update,
                "Settings",
                $"Updated max discount from {currentMaxDiscount:N2} to {model.MaxDiscount:N2}");
        }

        if (currentAllowDiscount != model.AllowDiscount)
        {
            await _auditService.LogAsync(
                AuditActionType.Update,
                "Settings",
                $"Changed discount permission to {(model.AllowDiscount ? "enabled" : "disabled")}");
        }

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
