using ClinicManagementSystem.DTOs.Patients;

namespace ClinicManagementSystem.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IPatientMedicalHistoryService _medicalHistoryService;
    private readonly IPatientHistoryService _patientHistoryService;
    private readonly IAuditService _auditService; 
    private readonly IExcelService _excelService;

    public PatientsController(
        IPatientService patientService,
        IPatientMedicalHistoryService medicalHistoryService,
        IPatientHistoryService patientHistoryService,
        IAuditService auditService,
        IExcelService excelService)
    {
        _patientService = patientService;
        _medicalHistoryService = medicalHistoryService;
        _patientHistoryService = patientHistoryService;
        _auditService = auditService;
        _excelService = excelService;
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

    public async Task<IActionResult> Details(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null)
            return NotFound();

        var viewModel = new PatientDetailsViewModel
        {
            Patient = patient,
            HistorySummary = await _patientHistoryService.GetPatientSummaryAsync(id),
            VisitTimeline = await _patientHistoryService.GetPatientVisitsTimelineAsync(id)
        };

        return View(viewModel);
    }

    public async Task<IActionResult> GetHistorySnapshot(int patientId)
    {
        var medicalHistory = await _medicalHistoryService.GetPatientHistoryAsync(patientId);
        var model = new PatientHistorySnapshotViewModel
        {
            Summary = await _patientHistoryService.GetPatientSummaryAsync(patientId),
            MedicalHistoryEntries = medicalHistory?.SummaryEntries ?? Array.Empty<PatientMedicalHistoryEntry>()
        };
        return PartialView("_PatientHistorySnapshot", model);
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
        var patientData = await _patientService.GetByIdAsync(patientId.Value);
        string actualPatientName = patientData?.Name ?? "مجهول";

        await _auditService.LogAsync(
            AuditActionType.Create,
            "MedicalHistory",
            $"تمت إضافة سجل مرضي جديد للمريض: {actualPatientName}",
            model.Id
        );

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

        var patientData = await _patientService.GetByIdAsync(patientId.Value);
        string actualPatientName = patientData?.Name ?? "مجهول";

        // 🔥 الـ Audit Log: تعديل السجل باسم المريض
        await _auditService.LogAsync(
            AuditActionType.Update,
            "MedicalHistory",
            $"تم تحديث السجل المرضي للمريض: {actualPatientName}",
            id
                );

        TempData["SuccessMessage"] = "تم تحديث السجل المرضي بنجاح";
        return RedirectToAction(nameof(History), new { id = patientId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteHistoryEntry(int id)
    {
        var currentEntry = await _medicalHistoryService.BuildEditFormAsync(id);
        string deletedPatientName = currentEntry?.PatientName ?? "مجهول";

        var patientId = await _medicalHistoryService.DeleteAsync(id);
        if (!patientId.HasValue)
        {
            return NotFound();
        }

        // 🔥 الـ Audit Log: حذف السجل باسم المريض
        await _auditService.LogAsync(
            AuditActionType.Delete,
            "MedicalHistory",
            $"تم حذف السجل المرضي للمريض: {deletedPatientName}",
            id
                );

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
    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        // 1. جلب المرضى الفعالين فقط (غير المحذوفين)
        var patients = await _patientService.SearchAsync(null); // جلب كل المرضى

        // 2. تحويل البيانات للـ DTO العربي
        var excelData = patients.Select(p => new PatientExcelDto
        {
            الكود = p.Id,
            الاسم = p.Name,
            الهاتف = p.Phone ?? "-",
            الجنس = p.Gender.ToString(),
            العمر = p.Age,
        }).ToList();

        // 3. توليد الملف
        var fileBytes = _excelService.ExportExcel(excelData, "قائمة المرضى");
        string fileName = $"Patients_Report_{DateTime.Now:yyyyMMdd}.xlsx";

        // 4. إرجاع الملف للتحميل فوراً
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}