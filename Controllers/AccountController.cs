namespace ClinicManagementSystem.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public AccountController(IAuthService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await _authService.ValidateUserAsync(model.Username, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var principal = await _authService.CreatePrincipalAsync(model.Username);
        if (principal == null)
        {
            ModelState.AddModelError(string.Empty, "User account is not active.");
            return View(model);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = model.RememberMe });

        HttpContext.User = principal;

        await _auditService.LogAsync(
            AuditActionType.Login,
            "Authentication",
            $"User {model.Username} logged in");

        return LocalRedirect(returnUrl ?? Url.Action("Index", "Dashboard")!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name ?? "Unknown";

        await _auditService.LogAsync(
            AuditActionType.Logout,
            "Authentication",
            $"User {username} logged out");

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
