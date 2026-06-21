namespace ClinicManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly IOperationalDataService _operationalDataService;
    private readonly IAuditService _auditService;
    private readonly IBackupService _backupService; // 🎯 حقن الـ Backup Service
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SettingsController(
        ISettingsService settingsService,
        IOperationalDataService operationalDataService,
        IAuditService auditService,
        IBackupService backupService,
        IWebHostEnvironment webHostEnvironment
        )
    {
        _settingsService = settingsService;
        _operationalDataService = operationalDataService;
        _auditService = auditService;
        _backupService = backupService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var model = new SettingsViewModel
        {
            ClinicName = await _settingsService.GetValue(SettingKeys.ClinicName) ?? string.Empty,
            ClinicPhone = await _settingsService.GetValue(SettingKeys.ClinicPhone) ?? string.Empty,
            ClinicAddress = await _settingsService.GetValue(SettingKeys.ClinicAddress) ?? string.Empty,
            ClinicLogoPath = await _settingsService.GetValue( SettingKeys.ClinicLogoPath) ?? string.Empty,
            ReceiptFooter = await _settingsService.GetValue(SettingKeys.ReceiptFooter) ?? string.Empty,
            DefaultExamPrice = await _settingsService.GetDecimal(SettingKeys.DefaultExamPrice),
            MaxDiscount = await _settingsService.GetDecimal(SettingKeys.MaxDiscount),
            AllowDiscount = await _settingsService.GetBool(SettingKeys.AllowDiscount),

            // 🎯 جلب قائمة النسخ الاحتياطية لعرضها في الجدول أسفل الصفحة
            Backups = await _backupService.GetBackupsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // إعادة تحميل قائمة الباك أبس لتجنب ظهور الجدول فارغاً لو الـ Validation فشل
            model.Backups = await _backupService.GetBackupsAsync();
            return View(model);
        }
        // 🎯 1. لو الدكتور رفع لوجو جديد، بنسيفه في الـ wwwroot
        if (model.LogoFile != null && model.LogoFile.Length > 0)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LogoFile.CopyToAsync(fileStream);
                }

                model.ClinicLogoPath = "/uploads/" + uniqueFileName;
                await _settingsService.SetValue(SettingKeys.ClinicLogoPath, model.ClinicLogoPath);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"فشل حفظ صورة الشعار: {ex.Message}");
                model.Backups = await _backupService.GetBackupsAsync();
                return View(model);
            }
        }
        else
        {
            // الاحتفاظ بالمسار القديم لو مرفعش صورة جديدة
            model.ClinicLogoPath = await _settingsService.GetValue(SettingKeys.ClinicLogoPath) ?? string.Empty;
        }

        var currentClinicName = await _settingsService.GetValue(SettingKeys.ClinicName) ?? string.Empty;
        var currentClinicPhone = await _settingsService.GetValue(SettingKeys.ClinicPhone) ?? string.Empty;
        var currentClinicAddress = await _settingsService.GetValue(SettingKeys.ClinicAddress) ?? string.Empty;
        var currentReceiptFooter = await _settingsService.GetValue(SettingKeys.ReceiptFooter) ?? string.Empty;
        var currentDefaultExamPrice = await _settingsService.GetDecimal(SettingKeys.DefaultExamPrice);
        var currentMaxDiscount = await _settingsService.GetDecimal(SettingKeys.MaxDiscount);
        var currentAllowDiscount = await _settingsService.GetBool(SettingKeys.AllowDiscount);

        await _settingsService.SetValue(SettingKeys.ClinicName, model.ClinicName);
        await _settingsService.SetValue(SettingKeys.ClinicPhone, model.ClinicPhone);
        await _settingsService.SetValue(SettingKeys.ClinicAddress, model.ClinicAddress);
        await _settingsService.SetValue(SettingKeys.ReceiptFooter, model.ReceiptFooter);
        await _settingsService.SetValue(SettingKeys.DefaultExamPrice, model.DefaultExamPrice.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _settingsService.SetValue(SettingKeys.MaxDiscount, model.MaxDiscount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _settingsService.SetValue(SettingKeys.AllowDiscount, model.AllowDiscount.ToString().ToLowerInvariant());

        if (!string.Equals(currentClinicName, model.ClinicName, StringComparison.Ordinal))
        {
            await _auditService.LogAsync(AuditActionType.Update, "Settings", "Changed clinic name");
        }

        if (!string.Equals(currentClinicPhone, model.ClinicPhone, StringComparison.Ordinal) ||
        !string.Equals(currentClinicAddress, model.ClinicAddress, StringComparison.Ordinal))
        {
            await _auditService.LogAsync(AuditActionType.Update, "Settings", "Updated clinic contact branding info (Phone/Address)");
        }
        if (currentDefaultExamPrice != model.DefaultExamPrice)
        {
            await _auditService.LogAsync(AuditActionType.Update, "Settings", $"Updated default examination price from {currentDefaultExamPrice:N2} to {model.DefaultExamPrice:N2}");
        }

        if (currentMaxDiscount != model.MaxDiscount)
        {
            await _auditService.LogAsync(AuditActionType.Update, "Settings", $"Updated max discount from {currentMaxDiscount:N2} to {model.MaxDiscount:N2}");
        }

        if (currentAllowDiscount != model.AllowDiscount)
        {
            await _auditService.LogAsync(AuditActionType.Update, "Settings", $"Changed discount permission to {(model.AllowDiscount ? "enabled" : "disabled")}");
        }

        TempData["SuccessMessage"] = "تم حفظ الإعدادات بنجاح";
        return RedirectToAction(nameof(Index));
    }

    // 🚀 1. عمل نسخة احتياطية جديدة يدوياً
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBackup()
    {
        try
        {
            var fileName = await _backupService.CreateBackupAsync();
            await _auditService.LogAsync(AuditActionType.Create, "Backup", $"تم إنشاء نسخة احتياطية يدوية بنجاح باسم: {fileName}");
            TempData["SuccessMessage"] = "تم إنشاء النسخة الاحتياطية بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل إنشاء النسخة الاحتياطية: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    // 🚀 2. استرجاع قاعدة البيانات من نسخة قديمة
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreBackup(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            TempData["ErrorMessage"] = "اسم ملف النسخة الاحتياطية غير صالح.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _backupService.RestoreBackupAsync(fileName);

            // 🎯 الحركة الصايعة: طالما الداتا رجعت لورا، يبقى الـ Cookies والـ Session الحاليين بتوع الآدمين ممكن يكونوا اختلفوا
            // هنمسح الكاش ونعمل Redirect صريح عشان الـ EF يجبر نفسه يفتح الاتصال الجديد
            TempData["SuccessMessage"] = "تم استرجاع البيانات بنجاح! تم تحديث قاعدة البيانات الحالية.";

            // نرجعه على الـ Index بس بـ Fresh Request
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل استرجاع البيانات: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // 🚀 3. مسح ملف نسخة احتياطية لتوفير مساحة
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBackup(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            TempData["ErrorMessage"] = "اسم ملف النسخة الاحتياطية غير صالح.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _backupService.DeleteBackupAsync(fileName);
            await _auditService.LogAsync(AuditActionType.Delete, "Backup", $"تم مسح ملف النسخة الاحتياطية: {fileName}");
            TempData["SuccessMessage"] = "تم حذف ملف النسخة الاحتياطية بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل حذف الملف: {ex.Message}";
        }
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