namespace ClinicManagementSystem.Middleware;

using Microsoft.AspNetCore.Http;
using ClinicManagementSystem.Services;
using System.Threading.Tasks;

public class LicenseMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // 🎯 التعديل الجوهري: السماح لأي ميثود داخل الـ LicenseController بالمرور (سواء عرض أو تفعيل)
        if (path.StartsWith("/license") || path.StartsWith("/setup"))
        {
            await _next(context);
            return;
        }

        // 2. فحص الامتدادات الشهيرة للملفات الثابتة
        if (path.StartsWith("/css/") ||
            path.StartsWith("/js/") ||
            path.StartsWith("/lib/") ||
            path.StartsWith("/images/") ||
            path.Contains("favicon.ico") ||
            path.EndsWith(".css") ||
            path.EndsWith(".js") ||
            path.EndsWith(".woff2") ||
            path.EndsWith(".svg") ||
            path.EndsWith(".png") ||
            path.EndsWith(".ico"))
        {
            await _next(context);
            return;
        }

        if (licenseService.IsExpired())
        {
            context.Response.Redirect("/License/Expired");
            return;
        }

        await _next(context);
    }
}