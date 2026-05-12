namespace ClinicManagementSystem.Controllers;

[Authorize]
public class DeletedController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;

    public DeletedController(IPatientService patientService, IDoctorService doctorService)
    {
        _patientService = patientService;
        _doctorService = doctorService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DeletedItemsViewModel
        {
            DeletedPatients = await _patientService.GetDeletedAsync(),
            DeletedDoctors = await _doctorService.GetDeletedAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestorePatient(int id)
    {
        await _patientService.RestoreAsync(id);
        TempData["SuccessMessage"] = "تم استرجاع المريض";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreDoctor(int id)
    {
        await _doctorService.RestoreAsync(id);
        TempData["SuccessMessage"] = "تم استرجاع الطبيب";
        return RedirectToAction(nameof(Index));
    }
}
