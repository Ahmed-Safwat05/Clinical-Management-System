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

    public VisitsController(
        IVisitService visitService,
        IPatientService patientService,
        IDoctorService doctorService,
        IProcedureService procedureService,
        ISettingsService settingsService,
        IVisitConsumptionService consumptionService,
        IProductService productService,
        IPaymentService paymentService)
    {
        _visitService = visitService;
        _patientService = patientService;
        _doctorService = doctorService;
        _procedureService = procedureService;
        _settingsService = settingsService;
        _consumptionService = consumptionService;
        _productService = productService;
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _visitService.GetRecentAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var visit = await _visitService.GetDetailsAsync(id);
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
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _visitService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف الزيارة بنجاح";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"حدث خطأ أثناء حذف الزيارة: {ex.Message}";
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
            TempData["SuccessMessage"] = "تم حذف الزيارة بنجاح";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"حدث خطأ أثناء حذف الزيارة: {ex.Message}";
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

    private async Task<VisitCreateViewModel> BuildCreateModelAsync(VisitCreateViewModel model)
    {
        var patients = await _patientService.SearchAsync(null);
        var doctors = await _doctorService.GetAllAsync();
        var procedures = await _procedureService.GetAllAsync();
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

        ViewData["MaxDiscount"] = maxDiscount;
        ViewData["AllowDiscount"] = allowDiscount.ToString().ToLowerInvariant();

        return model;
    }

    private async Task<VisitDetailsViewModel> BuildDetailsModelAsync(Visit visit)
    {
        var products = await _productService.GetAllAsync();
        var consumptions = await _consumptionService.GetVisitConsumptionsAsync(visit.Id);
        var totalConsumptionCost = await _consumptionService.GetTotalConsumptionCostAsync(visit.Id);

        return new VisitDetailsViewModel
        {
            Visit = visit,
            ProductConsumptions = consumptions,
            TotalConsumptionCost = totalConsumptionCost,
            AvailableProducts = products
                .Where(x => x.QuantityInStock > 0)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(
                    $"{x.Name} - المتاح: {x.QuantityInStock} {x.Unit} - التكلفة: {x.CostPrice:N2} ج.م",
                    x.Id.ToString()))
                .ToList()
        };
    }
}
