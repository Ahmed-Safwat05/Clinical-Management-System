namespace ClinicManagementSystem.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IPatientMedicalHistoryService _medicalHistoryService;

    public PatientsController(
        IPatientService patientService,
        IPatientMedicalHistoryService medicalHistoryService)
    {
        _patientService = patientService;
        _medicalHistoryService = medicalHistoryService;
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

    public async Task<IActionResult> History(int id)
    {
        var model = await _medicalHistoryService.GetPatientHistoryAsync(id);
        return model is null ? NotFound() : View(model);
    }

    public async Task<IActionResult> AddHistoryEntry(int patientId)
    {
        var model = await _medicalHistoryService.BuildCreateFormAsync(patientId);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddHistoryEntry(PatientMedicalHistoryEntryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var form = await _medicalHistoryService.BuildCreateFormAsync(model.PatientId);
            model.PatientName = form?.PatientName ?? model.PatientName;
            return View(model);
        }

        var patientId = await _medicalHistoryService.CreateAsync(model);
        if (!patientId.HasValue)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "تمت إضافة السجل المرضي بنجاح";
        return RedirectToAction(nameof(History), new { id = patientId.Value });
    }

    public async Task<IActionResult> EditHistoryEntry(int id)
    {
        var model = await _medicalHistoryService.BuildEditFormAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditHistoryEntry(int id, PatientMedicalHistoryEntryFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var form = await _medicalHistoryService.BuildEditFormAsync(id);
            model.PatientName = form?.PatientName ?? model.PatientName;
            return View(model);
        }

        var patientId = await _medicalHistoryService.UpdateAsync(model);
        if (!patientId.HasValue)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "تم تحديث السجل المرضي بنجاح";
        return RedirectToAction(nameof(History), new { id = patientId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteHistoryEntry(int id)
    {
        var patientId = await _medicalHistoryService.DeleteAsync(id);
        if (!patientId.HasValue)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "تم حذف السجل المرضي بنجاح";
        return RedirectToAction(nameof(History), new { id = patientId.Value });
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
