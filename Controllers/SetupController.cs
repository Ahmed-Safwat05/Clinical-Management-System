using DocumentFormat.OpenXml.InkML;

namespace ClinicManagementSystem.Controllers
{
    public class SetupController : Controller
    {
        private readonly ISettingsService _settingsService;
        private readonly IAuthService _authService;
        private readonly IAppUserRepository _userRepository;
        private readonly IWebHostEnvironment _environment;

        public SetupController(
            ISettingsService settingsService,
            IAuthService authService,
            IAppUserRepository userRepository,
            IWebHostEnvironment environment)
        {
            _settingsService = settingsService;
            _authService = authService;
            _userRepository = userRepository;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var isCompleted = await _settingsService.GetBool(SettingKeys.SetupCompleted);
            if (isCompleted)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompleteSetup(SetupViewModel model, IFormFile? clinicLogo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "يرجى التأكد من ملء جميع الحقول المطلوبة بشكل صحيح." });
                }

                // 1. معالجة وحفظ اللوجو (لو مرفعش، بنسيب المسار الافتراضي)
                string logoPath = "/uploads/logo.png";
                if (clinicLogo != null && clinicLogo.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = $"{Guid.NewGuid()}_{model.ClinicLogo.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    // 2. استخدام using لضمان غلق الـ Stream وتحرير الملف فوراً 🎯
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await model.ClinicLogo.CopyToAsync(fileStream);

                        // يفضل عمل Flush لضمان كتابة البيانات بالكامل قبل غلق البلوك
                        await fileStream.FlushAsync();
                    }
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await clinicLogo.CopyToAsync(stream);
                    }
                }

                // 2. حفظ إعدادات العيادة (الشاشة 2)
                await _settingsService.SetValue(SettingKeys.ClinicName, model.ClinicName);
                await _settingsService.SetValue(SettingKeys.ClinicPhone, model.ClinicPhone);
                await _settingsService.SetValue(SettingKeys.ClinicAddress, model.ClinicAddress);
                await _settingsService.SetValue(SettingKeys.ClinicTagLine, model.ClinicTagLine ?? "");
                await _settingsService.SetValue(SettingKeys.ReceiptFooter, model.ReceiptFooter);
                await _settingsService.SetValue(SettingKeys.ClinicLogoPath, logoPath);

                // 🎯 حفظ الإعدادات المالية (الشاشة 4 الجديد)
                await _settingsService.SetValue(SettingKeys.DefaultExamPrice, model.DefaultExamPrice.ToString());
                await _settingsService.SetValue(SettingKeys.MaxDiscount, model.MaxDiscount.ToString());
                await _settingsService.SetValue(SettingKeys.AllowDiscount, model.AllowDiscount.ToString().ToLower());

                // 3. إنشاء حساب المدير (الشاشة 3)
                var adminUser = new AppUser
                {
                    Username = model.AdminUsername,
                    DisplayName = model.AdminDisplayName,
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                adminUser.PasswordHash = _authService.HashPassword(adminUser, model.AdminPassword);
                await _userRepository.AddAsync(adminUser);

                // 4. إعلان اكتمال الإعداد بنجاح
                await _settingsService.SetValue(SettingKeys.SetupCompleted, "true");

                // 🎯 5. التريكة الذهبية: تسجيل دخول تلقائي فوراً للمدير بدون ما يشوف شاشة الـ Login
                var principal = await _authService.CreatePrincipalAsync(model.AdminUsername);
                if (principal != null)
                {
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                }

                return Json(new { success = true, redirectUrl = "/Home/Index" });
            }
            catch (DbUpdateException ex)
            {
                // 🎯 هنا بنجيب الرسالة الداخلية اللي فيها الخلاصة
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return Json(new { success = false, message = "خطأ في قاعدة البيانات: " + innerMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ غير متوقع: " + ex.Message });
            }
        }
    }
}