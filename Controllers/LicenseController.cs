namespace ClinicManagementSystem.Controllers
{
    public class LicenseController : Controller
    {
        private readonly ILicenseService _licenseService;

        public LicenseController(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        [HttpGet]
        public IActionResult Expired()
        {
            var licenseInfo = _licenseService.GetCurrentLicenseInfo();
            return View(licenseInfo);
        }

        [HttpPost]
        public IActionResult Activate(string activationCode)
        {
            if (string.IsNullOrWhiteSpace(activationCode))
            {
                TempData["ErrorMessage"] = "من فضلك أدخل كود التفعيل أولاً.";
                return RedirectToAction("Expired");
            }

            // محاولة تفعيل السيستم بالحزمة المشفرة
            if (_licenseService.ActivateSystemWithPackage(activationCode, out string errorMessage))
            {
                // 🎉 نجاح التفعيل! طيران على الـ Dashboard
                return RedirectToAction("Index", "Dashboard");
            }

            // لو فشل التفعيل، بنرجع الصفحة وبنعرض سبب الفشل (سواء تشفير أو بصمة جهاز غلط)
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction("Expired");
        }
    }
}