namespace ClinicManagementSystem.Controllers;

[Authorize]
public class VisitsController : Controller
{
    private readonly IVisitService _visitService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly IProcedureService _procedureService;
    private readonly ISettingsService _settingsService;
    private readonly IVisitConsumptionService _consumptionService;
    private readonly IProductService _productService;
    private readonly IPaymentService _paymentService;
    private readonly IPatientHistoryService _patientHistoryService;
    private readonly IPatientMedicalHistoryService _medicalHistoryService;
    private readonly IPrescriptionItemService _prescriptionItemService;

    public VisitsController(
        IVisitService visitService,
        IPatientService patientService,
        IDoctorService doctorService,
        IProcedureService procedureService,
        ISettingsService settingsService,
        IVisitConsumptionService consumptionService,
        IProductService productService,
        IPaymentService paymentService,
        IPatientHistoryService patientHistoryService,
        IPatientMedicalHistoryService medicalHistoryService,
        IPrescriptionItemService prescriptionItemService)
    {
        _visitService = visitService;
        _patientService = patientService;
        _doctorService = doctorService;
        _procedureService = procedureService;
        _settingsService = settingsService;
        _consumptionService = consumptionService;
        _productService = productService;
        _paymentService = paymentService;
        _patientHistoryService = patientHistoryService;
        _medicalHistoryService = medicalHistoryService;
        _prescriptionItemService = prescriptionItemService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _visitService.GetRecentAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var visit = await _visitService.GetDetailsAsync(id);
        if (visit == null)
        {
            return NotFound();
        }
        return visit is null ? NotFound() : View(await BuildDetailsModelAsync(visit));
    }

    public async Task<IActionResult> AddPayment(int visitId)
    {
        var model = await _paymentService.BuildCreateModelAsync(visitId);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(PaymentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var rebuiltModel = await _paymentService.BuildCreateModelAsync(model.VisitId);
            if (rebuiltModel is null)
            {
                return NotFound();
            }

            rebuiltModel.Amount = model.Amount;
            rebuiltModel.Notes = model.Notes;
            return View(rebuiltModel);
        }

        try
        {
            await _paymentService.AddPaymentAsync(model);
            TempData["SuccessMessage"] = "تمت إضافة الدفعة بنجاح";
            return RedirectToAction(nameof(Details), new { id = model.VisitId });
        }
        catch (ValidationException ex)
        {
            var rebuiltModel = await _paymentService.BuildCreateModelAsync(model.VisitId);
            if (rebuiltModel is null)
            {
                return NotFound();
            }

            rebuiltModel.Amount = model.Amount;
            rebuiltModel.Notes = model.Notes;
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(rebuiltModel);
        }
    }

    public async Task<IActionResult> Create(int? appointmentId)
    {
        var model = appointmentId.HasValue
            ? await _visitService.BuildFromAppointmentAsync(appointmentId.Value)
            : new VisitCreateViewModel();

        return model is null ? NotFound() : View(await BuildCreateModelAsync(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VisitCreateViewModel model)
    {
        model.Procedures = model.Procedures
            .Where(x => x.ProcedureId > 0 && x.Quantity > 0)
            .ToList() ?? new List<VisitProcedureInput>();
        model.ConsumedProducts = model.ConsumedProducts
            .Where(x => x.ProductId > 0 && x.Quantity > 0)
            .ToList() ?? new List<VisitProductConsumptionInput>();

        ModelState.Clear();
        if (!TryValidateModel(model))
        {
            return View(await BuildCreateModelAsync(model));
        }

        try
        {
            await _visitService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildCreateModelAsync(model));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildCreateModelAsync(model));
        }
    }

    public async Task<IActionResult> VoidConfirm(int id)
    {
        var visit = await _visitService.GetDetailsAsync(id);
        if (visit is null)
        {
            return NotFound();
        }

        if (visit.Status == VisitStatus.Voided)
        {
            TempData["ErrorMessage"] = "هذه الزيارة مُلغاة بالفعل";
            return RedirectToAction(nameof(Index));
        }

        return PartialView("_VoidConfirmModal", visit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VoidConfirm(int id, string? voidReason)
    {
        var visit = await _visitService.GetDetailsAsync(id);
        if (visit is null)
        {
            return NotFound();
        }

        if (visit.Status == VisitStatus.Voided)
        {
            return Json(new { success = false, message = "هذه الزيارة مُلغاة بالفعل" });
        }

        try
        {
            await _visitService.VoidAsync(id, voidReason);
            return Json(new { success = true, message = "تم إلغاء الزيارة بنجاح" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"حدث خطأ أثناء إلغاء الزيارة: {ex.Message}" });
        }
    }
    [HttpGet]
    public async Task<IActionResult> PrintSummary(int id)
    {
        // 🎯 بنستخدم الـ Service اللي مجهزة عندك ومحقونة في الكنترولر ومفيش أي حاجة هتضرب
        var visit = await _visitService.GetDetailsAsync(id);

        if (visit == null)
        {
            return NotFound("الزيارة غير موجودة.");
        }

        return View(visit);
    }
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _visitService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم إلغاء الزيارة بنجاح";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"حدث خطأ أثناء إلغاء الزيارة: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _visitService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم إلغاء الزيارة بنجاح";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"حدث خطأ أثناء إلغاء الزيارة: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConsumeProduct(int visitId, int productId, int quantity)
    {
        if (quantity <= 0)
        {
            TempData["ErrorMessage"] = "الكمية يجب أن تكون أكبر من صفر";
            return RedirectToAction(nameof(Details), new { id = visitId });
        }

        try
        {
            var visit = await _visitService.GetDetailsAsync(visitId);
            if (visit == null)
            {
                return NotFound();
            }

            await _consumptionService.ConsumeProductAsync(visitId, productId, quantity);
            TempData["SuccessMessage"] = "تم استهلاك المنتج بنجاح";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "فشل في استهلاك المنتج: " + ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = visitId });
    }
    [HttpPost]
    public async Task<IActionResult> AddPrescriptionItem(PrescriptionItem model)
    {
        ModelState.Remove("Visit");
        ModelState.Remove("Id");

        if (!ModelState.IsValid) return Json(new { success = false, message = "بيانات غير صالحة" });

        try
        {
            var result = await _prescriptionItemService.CreateAsync(model); // 👈 الميثود الجديدة

            if (result != null)
            {
                return Json(new
                {
                    success = true,
                    data = new { id = result.Id, medicationName = result.MedicationName, dosage = result.Dosage, frequency = result.Frequency, duration = result.Duration, notes = result.Notes }
                });
            }
            return Json(new { success = false, message = "لم يتم حفظ الدواء." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePrescriptionItem(PrescriptionItem model)
    {
        ModelState.Remove("Visit");

        if (!ModelState.IsValid) return Json(new { success = false, message = "بيانات غير صالحة" });

        try
        {
            var success = await _prescriptionItemService.UpdateAsync(model); // 👈 الميثود الجديدة

            if (success)
            {
                return Json(new
                {
                    success = true,
                    data = new { id = model.Id, medicationName = model.MedicationName, dosage = model.Dosage, frequency = model.Frequency, duration = model.Duration, notes = model.Notes }
                });
            }
            return Json(new { success = false, message = "حدث خطأ أثناء تحديث بيانات الدواء." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    [HttpPost]
    public async Task<IActionResult> DeletePrescriptionItem(int id)
    {
        try
        {
            var success = await _prescriptionItemService.DeleteAsync(id);
            if (success)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "لم يتم العثور على الدواء أو فشل الحذف." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    

    [HttpPost]
    private async Task<VisitCreateViewModel> BuildCreateModelAsync(VisitCreateViewModel model)
    {
        var patients = await _patientService.SearchAsync(null);
        var doctors = await _doctorService.GetAllAsync();
        var procedures = await _procedureService.GetAllAsync();
        var products = await _productService.GetAllAsync();
        var defaultExamPrice = await _settingsService.GetDecimal(SettingKeys.DefaultExamPrice);
        var maxDiscount = await _settingsService.GetDecimal(SettingKeys.MaxDiscount);
        var allowDiscount = await _settingsService.GetBool(SettingKeys.AllowDiscount);

        if (model.ExaminationPrice <= 0)
        {
            model.ExaminationPrice = defaultExamPrice;
        }

        model.Patients = patients.Select(x => new SelectListItem($"{x.Name} - {x.Phone}", x.Id.ToString())).ToList();
        model.Doctors = doctors.Select(x => new SelectListItem($"{x.Name} ({x.Specialty})", x.Id.ToString()));
        model.AvailableProcedures = procedures.Select(x => new SelectListItem($"{x.Name} - {x.Price:C}", x.Id.ToString()));
        model.AvailableProcedureOptions = procedures.Select(x => new ProcedureOptionViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Price = x.Price
        }).ToList();
        model.AvailableProductOptions = products
            .Where(x => x.QuantityInStock > 0)
            .OrderBy(x => x.Name)
            .Select(x => new ProductOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Unit = x.Unit,
                QuantityInStock = x.QuantityInStock,
                CostPrice = x.CostPrice
            })
            .ToList();

        ViewData["MaxDiscount"] = maxDiscount;
        ViewData["AllowDiscount"] = allowDiscount.ToString().ToLowerInvariant();

        return model;
    }

    private async Task<VisitDetailsViewModel> BuildDetailsModelAsync(Visit visit)
    {
        var products = await _productService.GetAllAsync();
        var consumptions = await _consumptionService.GetVisitConsumptionsAsync(visit.Id);
        var totalConsumptionCost = await _consumptionService.GetTotalConsumptionCostAsync(visit.Id);

        // 1. جلب السجل المرضي المزمن
        var medicalHistoryViewModel = await _medicalHistoryService.GetPatientHistoryAsync(visit.PatientId);
        var patientMedicalHistory = medicalHistoryViewModel?.Entries ?? Array.Empty<PatientMedicalHistoryEntry>();

        // 2. السحر هنا: جلب كل الزيارات السابقة للمريض (ترتيب من الأحدث للأقدم) باستثناء الحالية
        var allPatientVisits = await _visitService.GetRecentAsync(); // أو أي ميثود في الخدمة تجيب بالـ PatientId
                                                                     // لو الـ visitService مفيهاش ميثود فلترة، تقدر تكلم الـ DbContext مباشرة أو الفلترة هنا:
        var pastVisits = await _visitService.GetPatientVisitsAsync(visit.PatientId, visit.Id);

        pastVisits = pastVisits
            .Where(x => x.Id != visit.Id)
            .ToList();

        return new VisitDetailsViewModel
        {
            Visit = visit,
            ProductConsumptions = consumptions,
            TotalConsumptionCost = totalConsumptionCost,
            PatientMedicalHistory = patientMedicalHistory,
            PastVisits = pastVisits, // تمرير الزيارات السابقة للـ View
            AvailableProducts = products
                .Where(x => x.QuantityInStock > 0)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(
                    $"{x.Name} - المتاح: {x.QuantityInStock} {x.Unit} - التكلفة: {x.CostPrice:N2} ج.م",
                    x.Id.ToString()))
                .ToList()
        };
    }
    [HttpGet]
    public async Task<IActionResult> GetPatientSummaryJson(int patientId)
    {
        // جلب الزيارات وترتيبها وأخذ أول 3 فقط قبل الـ Select لتحسين الأداء
        var rawVisits = await _visitService.GetRecentAsync();

        var visits = rawVisits
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.Date)
            .Take(3) // 🚀 مكانها هنا أفضل عشان الـ Select يتنفذ على 3 عناصر فقط مش الكل
            .Select(v => new
            {
                id = v.Id, // 🎯 الـ Id اللي الجافا سكريبت محتاجه عشان زرار التفاصيل
                date = v.Date.ToString("yyyy-MM-dd"),
                doctor = v.Doctor?.Name ?? "غير محدد",
                notes = v.Notes ?? "لا توجد ملاحظات",
                price = v.TotalPrice
            });

        // جلب التاريخ المرضي والأمراض المزمنة
        var historyViewModel = await _medicalHistoryService.GetPatientHistoryAsync(patientId);
        var conditions = historyViewModel?.Entries?.Select(e => e.Title) ?? Array.Empty<string>();

        // إرجاع البيانات في الـ JSON
        return Json(new { visits, conditions });
    }
}
