using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ClinicManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IAppUserRepository _appUserRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IAppUserRepository appUserRepository, ILogger<UsersController> logger)
    {
        _appUserRepository = appUserRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _appUserRepository.GetAllAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check if username already exists (including inactive)
        var existingUser = await _appUserRepository.GetByUsernameIncludingInactiveAsync(model.Username);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Username), "اسم المستخدم موجود بالفعل.");
            return View(model);
        }

        var passwordHasher = new PasswordHasher<AppUser>();
        var newUser = new AppUser
        {
            Username = model.Username,
            DisplayName = model.DisplayName,
            Role = UserRole.Receptionist,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        newUser.PasswordHash = passwordHasher.HashPassword(newUser, model.Password);

        try
        {
            await _appUserRepository.AddAsync(newUser);
            TempData["SuccessMessage"] = "تم إنشاء المستخدم بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء إنشاء المستخدم.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _appUserRepository.GetByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "المستخدم غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        // Prevent deactivating the only admin
        if (user.Role == UserRole.Admin && user.IsActive)
        {
            var activeAdmins = (await _appUserRepository.GetAllAsync()).Count(u => u.Role == UserRole.Admin && u.IsActive);
            if (activeAdmins <= 1)
            {
                TempData["ErrorMessage"] = "لا يمكن تعطيل آخر مسؤول فعال.";
                return RedirectToAction(nameof(Index));
            }
        }

        user.IsActive = !user.IsActive;
        try
        {
            await _appUserRepository.UpdateAsync(user);
            var status = user.IsActive ? "تم تفعيل المستخدم" : "تم تعطيل المستخدم";
            TempData["SuccessMessage"] = status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user active status");
            TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث حالة المستخدم.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword(int id)
    {
        // Users can only change their own password or admins can change any
        var nameIdentifierValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(nameIdentifierValue, out var currentUserId))
        {
            return Unauthorized();
        }

        if (id != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _appUserRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(new ChangePasswordViewModel { UserId = id, DisplayName = user.DisplayName });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(int id, ChangePasswordViewModel model)
    {
        var nameIdentifierValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(nameIdentifierValue, out var currentUserId))
        {
            return Unauthorized();
        }

        if (id != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // Validate CurrentPassword requirement for non-admin self-changes
        if (id == currentUserId && string.IsNullOrEmpty(model.CurrentPassword))
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "كلمة المرور الحالية مطلوبة.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _appUserRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // If not admin, verify current password
        if (id == currentUserId && !string.IsNullOrEmpty(model.CurrentPassword))
        {
            var passwordHasher = new PasswordHasher<AppUser>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "كلمة المرور الحالية غير صحيحة.");
                return View(model);
            }
        }

        // Update password
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, model.NewPassword);

        try
        {
            await _appUserRepository.UpdateAsync(user);
            TempData["SuccessMessage"] = "تم تحديث كلمة المرور بنجاح.";

            // If user changed their own password, they need to log in again
            if (id == currentUserId)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء تحديث كلمة المرور.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var nameIdentifierValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(nameIdentifierValue, out var currentUserId))
        {
            return Unauthorized();
        }

        // Prevent deactivating self
        if (id == currentUserId)
        {
            TempData["ErrorMessage"] = "لا يمكن تعطيل حسابك الخاص.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _appUserRepository.GetByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "المستخدم غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        // Prevent deactivating last active admin
        if (user.Role == UserRole.Admin && user.IsActive)
        {
            var activeAdmins = (await _appUserRepository.GetAllAsync()).Count(u => u.Role == UserRole.Admin && u.IsActive);
            if (activeAdmins <= 1)
            {
                TempData["ErrorMessage"] = "لا يمكن تعطيل آخر مسؤول فعال.";
                return RedirectToAction(nameof(Index));
            }
        }

        if (!user.IsActive)
        {
            TempData["ErrorMessage"] = "المستخدم معطل بالفعل.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            user.IsActive = false;
            await _appUserRepository.UpdateAsync(user);
            _logger.LogInformation($"User {id} ({user.Username}) deactivated by admin {currentUserId}");
            TempData["SuccessMessage"] = $"تم تعطيل المستخدم '{user.DisplayName}' بنجاح.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user");
            TempData["ErrorMessage"] = "حدث خطأ أثناء تعطيل المستخدم.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(int id)
    {
        var nameIdentifierValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(nameIdentifierValue, out var currentUserId))
        {
            return Unauthorized();
        }

        var user = await _appUserRepository.GetByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "المستخدم غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        if (user.IsActive)
        {
            TempData["ErrorMessage"] = "المستخدم نشط بالفعل.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            user.IsActive = true;
            await _appUserRepository.UpdateAsync(user);
            _logger.LogInformation($"User {id} ({user.Username}) reactivated by admin {currentUserId}");
            TempData["SuccessMessage"] = $"تم تفعيل المستخدم '{user.DisplayName}' بنجاح.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating user");
            TempData["ErrorMessage"] = "حدث خطأ أثناء تفعيل المستخدم.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var nameIdentifierValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(nameIdentifierValue, out var currentUserId))
        {
            return Unauthorized();
        }

        // Prevent deleting the currently logged-in user
        if (id == currentUserId)
        {
            TempData["ErrorMessage"] = "لا يمكن حذف حسابك الخاص. اطلب من مسؤول آخر حذفه.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _appUserRepository.GetByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "المستخدم غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        // Prevent deleting last active admin
        if (user.Role == UserRole.Admin && user.IsActive)
        {
            var activeAdmins = (await _appUserRepository.GetAllAsync()).Count(u => u.Role == UserRole.Admin && u.IsActive);
            if (activeAdmins <= 1)
            {
                TempData["ErrorMessage"] = "لا يمكن حذف آخر مسؤول فعال.";
                return RedirectToAction(nameof(Index));
            }
        }

        try
        {
            // For safety, mark as inactive instead of hard delete
            user.IsActive = false;
            await _appUserRepository.UpdateAsync(user);
            _logger.LogInformation($"User {id} ({user.Username}) deleted by admin {currentUserId}");
            TempData["SuccessMessage"] = $"تم حذف المستخدم '{user.DisplayName}' بنجاح.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            TempData["ErrorMessage"] = "حدث خطأ أثناء حذف المستخدم.";
        }

        return RedirectToAction(nameof(Index));
    }
}
