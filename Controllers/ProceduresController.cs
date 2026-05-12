using Azure.Core;

namespace ClinicManagementSystem.Controllers;

[Authorize]
public class ProceduresController : Controller
{
    private readonly IProcedureService _procedureService;

    public ProceduresController(IProcedureService procedureService)
    {
        _procedureService = procedureService;
    }

    public async Task<IActionResult> Index() => View(await _procedureService.GetAllAsync());

    public IActionResult Create() => View(new Procedure());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Procedure procedure)
    {
        if (!ModelState.IsValid)
        {
            // إذا كان الطلب AJAX وفشل الـ Validation، نرسل رسالة خطأ بدلاً من الـ View
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "بيانات الإجراء غير صالحة" });
            }
            return View(procedure);
        }

        await _procedureService.CreateAsync(procedure);

        // إذا كان الطلب AJAX (من صفحة الزيارة)، نرد ببيانات الـ JSON
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new
            {
                success = true,
                id = procedure.Id,
                name = procedure.Name,
                price = procedure.Price
            });
        }

        // الطلب الطبيعي يكمل مساره المعتاد
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var procedure = await _procedureService.GetByIdAsync(id);
        return procedure is null ? NotFound() : View(procedure);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Procedure procedure)
    {
        if (id != procedure.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(procedure);
        }

        await _procedureService.UpdateAsync(procedure);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _procedureService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
