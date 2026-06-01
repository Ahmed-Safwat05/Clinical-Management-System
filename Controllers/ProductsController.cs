namespace ClinicManagementSystem.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly IStockManagementService _stockManagementService;

    public ProductsController(IProductService productService, IStockManagementService stockManagementService)
    {
        _productService = productService;
        _stockManagementService = stockManagementService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
            return NotFound();

        var transactions = await _stockManagementService.GetProductTransactionsAsync(id);

        var model = new ProductDetailsViewModel
        {
            Product = product,
            Transactions = transactions,
            LowStockWarning = product.QuantityInStock <= product.MinimumQuantity
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStock(int id, int quantity, string reason)
    {
        if (quantity <= 0)
        {
            TempData["ErrorMessage"] = "الكمية يجب أن تكون أكبر من صفر";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            var success = await _stockManagementService.AddStockAsync(id, quantity, reason);
            if (success)
            {
                TempData["SuccessMessage"] = "تم إضافة الكمية بنجاح";
            }
            else
            {
                TempData["ErrorMessage"] = "فشل في إضافة الكمية";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStock(int id, int quantity, string reason)
    {
        if (quantity <= 0)
        {
            TempData["ErrorMessage"] = "الكمية يجب أن تكون أكبر من صفر";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            var success = await _stockManagementService.RemoveStockAsync(id, quantity, reason);
            if (success)
            {
                TempData["SuccessMessage"] = "تم سحب الكمية بنجاح";
            }
            else
            {
                TempData["ErrorMessage"] = "فشل في سحب الكمية";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    public IActionResult Create() => View(new Product());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        await _productService.CreateAsync(product);
        TempData["SuccessMessage"] = "تم إضافة المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Unit = product.Unit,
            CostPrice = product.CostPrice,
            MinimumQuantity = product.MinimumQuantity,
            IsActive = product.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var product = new Product
            {
                Id = model.Id,
                Name = model.Name,
                Unit = model.Unit,
                CostPrice = model.CostPrice,
                MinimumQuantity = model.MinimumQuantity,
                IsActive = model.IsActive
            };

            await _productService.UpdateAsync(product);
            TempData["SuccessMessage"] = "تم تحديث المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف المنتج بنجاح";
        }
        catch (InvalidOperationException)
        {
            TempData["ErrorMessage"] = "فشل في حذف المنتج";
        }
        return RedirectToAction(nameof(Index));
    }
}
