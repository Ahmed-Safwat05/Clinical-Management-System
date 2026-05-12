namespace ClinicManagementSystem.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        ViewData["SearchTerm"] = searchTerm;
        return View(await _patientService.SearchAsync(searchTerm));
    }

    public IActionResult Create() => View(new Patient());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient patient)
    {
        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        await _patientService.CreateAsync(patient);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        return patient is null ? NotFound() : View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Patient patient)
    {
        if (id != patient.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        await _patientService.UpdateAsync(patient);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _patientService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف المريض بنجاح";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"حدث خطأ أثناء حذف المريض: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}
